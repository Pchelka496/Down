using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using Zenject;

public class EngineModule : BaseModule, IFlightModule
{
    const float START_FORCE_VALUE = 0f;
    const float START_APPLY_RATE_VALUE = 0.5f;
    const float MIN_VECTOR_LENGHT_FOR_BOOST = 10f;
    const float PARTICLES_START_SPEED_MIN = 1f;
    const float PARTICLES_START_SPEED_MAX = 15f;

    const float BOOST_ACTIVATION_TIME = 0.5f;
    const float ANGLE_THRESHOLD = 2f;

    [SerializeField] Transform _engineRotationCenter;
    [SerializeField] Transform _engine;
    [SerializeField] ParticleSystem _defaultParticleSystem;
    [SerializeField] ParticleSystem _boostParticleSystem;

    float _boosterForce = 1000f;
    float _maxForce = 50f;
    float _interpolateForceApplyRateValue = 0.1f;
    float _engineForceIncreaseDuration = 1f;
    float _maxForceApplyRate = 0.02f;
    float _rotationSpeed = 200f;
    float _currentEngineForce;
    float _forceApplyRate = 0.2f;
    bool _needBoost;

    float _minVectorLength = 0f;
    float _maxVectorLength = Screen.width * 0.25f;

    Rigidbody2D _rb;
    Controls _controls;
    ScreenTouchController _screenTouchController;

    Tween _increaseForceTween;

    CancellationTokenSource _rotationCts;
    CancellationTokenSource _boostCts;
    CancellationTokenSource _defaultEngineCts;

    [Inject]
    private void Construct(CharacterController player, Controls controls, ScreenTouchController screenTouchController, LevelManager levelManager, EngineModuleConfig engineModuleConfig)
    {
        _rb = player.Rb;
        _controls = controls;
        _screenTouchController = screenTouchController;
        SnapToPlayer(player.transform);

        levelManager.SubscribeToRoundStart(RoundStart);

        player.FlightModule = this;

        _controls.Player.TouchScreen.performed += ctx => StartTouchScreen();
        _controls.Player.TouchScreen.canceled += ctx => StopTouch();

        UpdateCharacteristics(engineModuleConfig);
        ResetEngineRotation();
        ResetValues();
    }

    private void RoundStart(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundEnd(RoundEnd);
        UpdateCharacteristics();
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
        ResetEngineRotation();
    }

    private void ResetEngineRotation()
    {
        _engineRotationCenter.rotation = Quaternion.Euler(0f, 0f, -90f);
    }

    private void UpdateCharacteristics() => UpdateCharacteristics(GameplaySceneInstaller.DiContainer.Resolve<EngineModuleConfig>());

    private void UpdateCharacteristics(EngineModuleConfig engineModuleConfig)
    {
        _maxForce = engineModuleConfig.EngineMaxForce;
        _boosterForce = engineModuleConfig.BoostPower;
        _engineForceIncreaseDuration = engineModuleConfig.EngineForceIncreaseDuration;
        _maxForceApplyRate = engineModuleConfig.ApplyForceRate;
        _interpolateForceApplyRateValue = engineModuleConfig.InterpolateForceApplyRateValue;

        if (_maxForceApplyRate == 0 || _maxForceApplyRate > 0)
        {
            _maxForceApplyRate = 0.02f;
        }

        _rotationSpeed = engineModuleConfig.EngineRotationSpeed;

        UpdateEngineParticles();
        _defaultParticleSystem.Stop();
    }

    private void StartTouchScreen()
    {
        ClearToken(ref _rotationCts);
        ClearToken(ref _boostCts);

        _rotationCts = new();
        _boostCts = new();

        BoostHandler(_boostCts.Token).Forget();
        RotateEngineTowardsTouch(_rotationCts.Token, _boostCts.Token).Forget();
    }

    private async UniTask BoostHandler(CancellationToken token)
    {
        _needBoost = true;

        await UniTask.WaitForSeconds(BOOST_ACTIVATION_TIME, cancellationToken: token);

        _needBoost = false;

        ClearToken(ref _defaultEngineCts);
        _defaultEngineCts = new();

        DefaultEngineWork(_defaultEngineCts.Token).Forget();
    }

    private void StopTouch()
    {
        ClearToken(ref _rotationCts);
        ClearToken(ref _boostCts);
        ClearToken(ref _defaultEngineCts);
    }

    private async UniTask RotateEngineTowardsTouch(CancellationToken rotationToken, CancellationToken boosterToken)
    {
        var currentRotationSpeed = 0f;
        var maxRotationSpeed = _rotationSpeed;
        var acceleration = 0.5f;

        var startTouch = _screenTouchController.TouchStartPosition;

        await UniTask.WaitUntil(() => Vector2.Distance(startTouch, _screenTouchController.TouchCurrentPosition) > 0.1f, cancellationToken: rotationToken);

        while (!rotationToken.IsCancellationRequested || _needBoost)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);

            var currentTouch = _screenTouchController.TouchCurrentPosition;
            var direction = startTouch - currentTouch;
            var targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            var currentAngle = _engineRotationCenter.eulerAngles.z;
            var angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);

            if (Mathf.Abs(angleDifference) <= ANGLE_THRESHOLD)
            {
                if (_needBoost & boosterToken.IsCancellationRequested)
                {
                    if (Vector2.Distance(startTouch, currentTouch) > MIN_VECTOR_LENGHT_FOR_BOOST)
                    {
                        ApplyBoost();
                        _needBoost = false;
                    }
                }

                continue;
            }

            currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, maxRotationSpeed, acceleration * Time.deltaTime);
            var newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, currentRotationSpeed * Time.deltaTime);
            _engineRotationCenter.rotation = Quaternion.Euler(0, 0, newAngle);
        }
    }

    private void ApplyBoost()
    {
        ApplyForce(_engine, _boosterForce);
        _boostParticleSystem.Play();
        _needBoost = false;
    }

    public async UniTask DefaultEngineWork(CancellationToken token)
    {
        var startTouch = _screenTouchController.TouchStartPosition;
        var currentTouch = Vector2.zero;
        var currentVectorLength = 0f;
        var normalizedForce = 0f;
        var force = 0f;

        StartIncreaseForceTween();

        while (!token.IsCancellationRequested)
        {
            currentTouch = _screenTouchController.TouchCurrentPosition;
            currentVectorLength = Vector2.Distance(startTouch, currentTouch);

            currentVectorLength = Mathf.Clamp(currentVectorLength, _minVectorLength, _maxVectorLength);

            normalizedForce = Mathf.InverseLerp(_minVectorLength, _maxVectorLength, currentVectorLength);

            force = _currentEngineForce * normalizedForce;

            ApplyForce(_engine, force);
            UpdateEngineParticles(force);

            if (_forceApplyRate - _maxForceApplyRate > 0.01f)
            {
                _forceApplyRate = Mathf.Lerp(_forceApplyRate, _maxForceApplyRate, _interpolateForceApplyRateValue);
            }
            else
            {
                _forceApplyRate = _maxForceApplyRate;
            }

            await UniTask.WaitForSeconds(_forceApplyRate);
        }

        StopTween();
        ResetValues();
    }

    private void StartIncreaseForceTween()
    {
        _increaseForceTween = DOTween.To(() => _currentEngineForce, x => _currentEngineForce = x, _maxForce, _engineForceIncreaseDuration)
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
        UpdateEngineParticles(0f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ApplyForce(Transform engine, float force)
    {
        var engineRotationZ = engine.eulerAngles.z + 90f;

        var forceDirection = new Vector2(-Mathf.Cos(engineRotationZ * Mathf.Deg2Rad), -Mathf.Sin(engineRotationZ * Mathf.Deg2Rad));
        _rb.AddForceAtPosition(forceDirection * force, engine.position);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateEngineParticles(float currentForce = 0f)
    {
        var main = _defaultParticleSystem.main;
        var emission = _defaultParticleSystem.emission;

        var rateMultiplier = 1f / _forceApplyRate;
        emission.rateOverTime = rateMultiplier;

        main.startSpeed = Mathf.Lerp(PARTICLES_START_SPEED_MIN, PARTICLES_START_SPEED_MAX, currentForce / _maxForce);

        if (currentForce > 0 && !_defaultParticleSystem.isPlaying)
        {
            _defaultParticleSystem.Play();
        }
        else if (currentForce <= 0 && _defaultParticleSystem.isPlaying)
        {
            _defaultParticleSystem.Stop();
        }
    }

    private void ClearToken(ref CancellationTokenSource cts) => ClearTokenSupport.ClearToken(ref cts);

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_engine != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_engine.position, _engine.up * 2);
        }
    }
#endif

}

