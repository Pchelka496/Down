using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Additional;
using Core.Installers;
using Creatures.Player.FlightModule.EngineModule;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ScriptableObject.ModulesConfig.FlightModule;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Creatures.Player.PlayerModule.CoreModules.EngineModule
{
    public class EngineModule : BaseModule
    {
        const float ROTATION_Z_OFFSET = -90f;
        const float MIN_VECTOR_LENGTH_FOR_BOOST = 33f;
        const float MIN_VECTOR_LENGTH_FOR_DEFAULT_ENGINE_WORK = 0f;
        const float START_FORCE_VALUE = 0f;
        const float START_APPLY_RATE_VALUE = 0.5f;

        const float BOOST_ACTIVATION_TIME = 0.4f;

        [SerializeField] Transform _engineRotationCenter;
        [SerializeField] Transform _engine;
        [SerializeField] EngineModuleVisualPart _visualPart;

        [SerializeField] float _maxForce = 50f;
        [SerializeField] float _interpolateForceApplyRateValue = 0.1f;
        [SerializeField] float _engineForceIncreaseDuration = 1f;
        [SerializeField] float _maxForceApplyRate = 0.02f;
        float _currentEngineForce;
        float _forceApplyRate = 0.2f;
        bool _needRotation;

        readonly float _maxVectorLength = Screen.width * 0.25f;

        readonly EngineModuleBooster _booster = new();
        RotationModule _rotationModule;
        Rigidbody2D _rb;
        Controls _controls;
        ScreenTouchController _screenTouchController;

        Tween _increaseForceTween;

        CancellationTokenSource _boostCts;
        CancellationTokenSource _defaultEngineCts;

        [Inject]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:������� �������������� �������� �����",
            Justification = "<��������>")]
        private void Construct(PlayerController player, Controls controls, ScreenTouchController screenTouchController,
            EngineModuleConfig engineModuleConfig, BoosterIndicator indicator)
        {
            _rb = player.Rb;
            _rotationModule = player.RotationModule;
            _controls = controls;
            _screenTouchController = screenTouchController;
            _booster.Initialize(_rb, _engine, _visualPart, indicator);

            SnapToPlayer(player.transform, Vector3.zero, Quaternion.Euler(0f, 0f, ROTATION_Z_OFFSET));

            SubscribeToControlsEvent();

            UpdateCharacteristics(engineModuleConfig);
            ResetValues();
        }

        public void UpdateCharacteristics() =>
            UpdateCharacteristics(GameplaySceneInstaller.DiContainer.Resolve<EngineModuleConfig>());

        public void UpdateCharacteristics(EngineModuleConfig engineModuleConfig)
        {
            _maxForce = engineModuleConfig.EngineMaxForce;
            _engineForceIncreaseDuration = engineModuleConfig.EngineForceIncreaseDuration;
            _maxForceApplyRate = engineModuleConfig.ApplyForceRate;
            _interpolateForceApplyRateValue = engineModuleConfig.InterpolateForceApplyRateValue;

            if (_maxForceApplyRate == 0 || _maxForceApplyRate > 0)
            {
                _maxForceApplyRate = 0.02f;
            }

            _booster.UpdateCharacteristics(engineModuleConfig.BoostPower,
                (int)engineModuleConfig.BoosterChargeCount,
                engineModuleConfig.BoosterChargeCooldown
            );

            _visualPart.UpdateDefaultWork(_maxForce, 0f, _forceApplyRate);
        }

        public override void EnableModule()
        {
            SubscribeToControlsEvent();
        }

        public override void DisableModule()
        {
            UnsubscribeToControlsEvent();
            ClearToken(ref _boostCts);
            ClearToken(ref _defaultEngineCts);
        }

        private void StartTouchScreen(InputAction.CallbackContext ctx)
        {
            ClearToken(ref _boostCts);

            _boostCts = new();

            EngineWorkHandler(_boostCts.Token).Forget();
        }

        private async UniTask EngineWorkHandler(CancellationToken token)
        {
            _rotationModule.ClearOnTargetRotationReached();
            _needRotation = true;

            ClearToken(ref _defaultEngineCts);
            _defaultEngineCts = new();

            DefaultEngineWork(_defaultEngineCts.Token).Forget();

            try
            {
                await UniTask.WaitForSeconds(BOOST_ACTIVATION_TIME, cancellationToken: token);
            }
            catch (OperationCanceledException) //Use boost
            {
                var startTouch = _screenTouchController.TouchStartPosition;
                var currentTouch = _screenTouchController.TouchCurrentPosition;

                var currentVectorLength = Vector2.Distance(startTouch, currentTouch);

                if (MIN_VECTOR_LENGTH_FOR_BOOST <= currentVectorLength)
                {
                    _rotationModule.SetOnTargetRotationReached(_booster.ApplyBoost);
                }

                _needRotation = false;
            }
        }

        private bool NeedRotation() => _needRotation;

        private void StopTouch(InputAction.CallbackContext ctx)
        {
            ClearToken(ref _boostCts);
            ClearToken(ref _defaultEngineCts);
        }

        private async UniTask DefaultEngineWork(CancellationToken token)
        {
            StopTween();
            ResetValues();

            StartIncreaseForceTween();

            var startTouch = _screenTouchController.TouchStartPosition;

            while (!token.IsCancellationRequested)
            {
                var currentTouch = _screenTouchController.TouchCurrentPosition;
                var currentVectorLength = Vector2.Distance(startTouch, currentTouch);

                currentVectorLength = Mathf.Clamp(currentVectorLength, MIN_VECTOR_LENGTH_FOR_DEFAULT_ENGINE_WORK,
                    _maxVectorLength);

                var normalizedForce = Mathf.InverseLerp(MIN_VECTOR_LENGTH_FOR_DEFAULT_ENGINE_WORK, _maxVectorLength,
                    currentVectorLength);

                var force = _currentEngineForce * normalizedForce;

                ApplyForce(_rb, _engine, force);
                _visualPart.UpdateDefaultWork(_maxForce, force, _forceApplyRate);

                _forceApplyRate = _forceApplyRate - _maxForceApplyRate > 0.01f
                    ? Mathf.Lerp(_forceApplyRate, _maxForceApplyRate, _interpolateForceApplyRateValue)
                    : _maxForceApplyRate;

                // ReSharper disable once MethodSupportsCancellation
                await UniTask.WaitForSeconds(_forceApplyRate);
            }

            _needRotation = false;
            ResetValues();
        }

        private void StartIncreaseForceTween()
        {
            _increaseForceTween = DOTween.To(() => _currentEngineForce, x => _currentEngineForce = x, _maxForce,
                    _engineForceIncreaseDuration)
                .SetEase(Ease.InOutQuad);
        }

        private void StopTween()
        {
            _increaseForceTween?.Kill();
        }

        private void ResetValues()
        {
            _currentEngineForce = START_FORCE_VALUE;
            _forceApplyRate = START_APPLY_RATE_VALUE;

            _visualPart.UpdateDefaultWork(_maxForce, 0f, _forceApplyRate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ApplyForce(Rigidbody2D rb, Transform engine, float force)
        {
            var engineRotationZ = engine.eulerAngles.z - EngineModule.ROTATION_Z_OFFSET;

            var forceDirection = new Vector2(-Mathf.Cos(engineRotationZ * Mathf.Deg2Rad),
                -Mathf.Sin(engineRotationZ * Mathf.Deg2Rad));
            rb.AddForceAtPosition(forceDirection * force, engine.position);
        }

        private void ClearToken(ref CancellationTokenSource cts) => ClearTokenSupport.ClearToken(ref cts);

        private void SubscribeToControlsEvent()
        {
            _controls.Player.TouchScreen.performed += StartTouchScreen;
            _controls.Player.TouchScreen.canceled += StopTouch;
            _rotationModule.SetToEngineNeedRotation(NeedRotation);
        }

        private void UnsubscribeToControlsEvent()
        {
            _controls.Player.TouchScreen.performed -= StartTouchScreen;
            _controls.Player.TouchScreen.canceled -= StopTouch;
            _rotationModule.ClearEngineNeedRotation();
        }

        private void OnDestroy()
        {
            ClearToken(ref _boostCts);
            ClearToken(ref _defaultEngineCts);

            _booster.Dispose();
            UnsubscribeToControlsEvent();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_engine != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(_engine.position, _engine.up * 3);
            }
        }
#endif
    }
}