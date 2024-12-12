using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Additional;
using Core.Installers;
using Creatures.Player;
using ScriptableObject.ModulesConfig.SupportModules;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class RotationModule : BaseModule
{
    const float PLAYER_ROTATION_OFFSET = 90f;
    const float ANGLE_THRESHOLD = 2f;

    [SerializeField] float _rotationSpeed;

    Transform _engineRotationCenter;
    Rigidbody2D _rb;

    Controls _controls;
    ScreenTouchController _screenTouchController;

    CancellationTokenSource _rotationCts;

    Func<bool> _engineNeedRotation = () => false;

    Action _onTargetRotationReached;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:������� �������������� �������� �����", Justification = "<��������>")]
    private void Construct(PlayerController player, Controls controls, ScreenTouchController screenTouchController, RotationModuleConfig config)
    {
        _rb = player.Rb;
        _engineRotationCenter = _rb.transform;
        _controls = controls;
        _screenTouchController = screenTouchController;

        SnapToPlayer(player.transform);

        SubscribeToControlsEvent();

        UpdateCharacteristics(config);
    }

    public void UpdateCharacteristics() => UpdateCharacteristics(GameplaySceneInstaller.DiContainer.Resolve<RotationModuleConfig>());

    public void UpdateCharacteristics(RotationModuleConfig config)
    {
        _rotationSpeed = config.RotationSpeed;
    }

    public override void EnableModule()
    {
        SubscribeToControlsEvent();
    }

    public override void DisableModule()
    {
        UnsubscribeToControlsEvent();
        ClearToken(ref _rotationCts);
    }

    private void StartTouchScreen(InputAction.CallbackContext ctx)
    {
        ClearToken(ref _rotationCts);
        _rotationCts = new();

        RotateEngineTowardsTouch(_rotationCts.Token).Forget();
    }

    private void StopTouch(InputAction.CallbackContext ctx)
    {
        ClearToken(ref _rotationCts);
    }

    private async UniTask RotateEngineTowardsTouch(CancellationToken rotationToken)
    {
        var currentRotationSpeed = 0f;
        var maxRotationSpeed = _rotationSpeed;
        var acceleration = 0.5f;

        var startTouch = _screenTouchController.TouchStartPosition;

        await UniTask.WaitUntil(() => Vector2.Distance(startTouch, _screenTouchController.TouchCurrentPosition) > 0.1f, cancellationToken: rotationToken);

        while (!rotationToken.IsCancellationRequested || _engineNeedRotation.Invoke() || _onTargetRotationReached != null)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);

            var currentTouch = _screenTouchController.TouchCurrentPosition;
            var direction = startTouch - currentTouch;
            var targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            var currentAngle = _engineRotationCenter.eulerAngles.z - PLAYER_ROTATION_OFFSET;
            var angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);

            if (Mathf.Abs(angleDifference) <= ANGLE_THRESHOLD)
            {
                _onTargetRotationReached?.Invoke();
                _onTargetRotationReached = null;

                continue;
            }

            currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, maxRotationSpeed, acceleration * Time.deltaTime);
            var newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, currentRotationSpeed * Time.deltaTime);
            _engineRotationCenter.rotation = Quaternion.Euler(0, 0, newAngle + PLAYER_ROTATION_OFFSET);
        }
    }

    public void SetOnTargetRotationReached(Action action) => _onTargetRotationReached = action;
    public void ClearOnTargetRotationReached() => _onTargetRotationReached = null;

    public void SetToEngineNeedRotation(Func<bool> func) => _engineNeedRotation = func;
    public void ClearEngineNeedRotation() => _engineNeedRotation = () => false;

    private void ClearToken(ref CancellationTokenSource cts) => ClearTokenSupport.ClearToken(ref cts);

    private void SubscribeToControlsEvent()
    {
        _controls.Player.TouchScreen.performed += StartTouchScreen;
        _controls.Player.TouchScreen.canceled += StopTouch;
    }

    private void UnsubscribeToControlsEvent()
    {
        _controls.Player.TouchScreen.performed -= StartTouchScreen;
        _controls.Player.TouchScreen.canceled -= StopTouch;
    }

    private void OnDestroy()
    {
        ClearToken(ref _rotationCts);
        UnsubscribeToControlsEvent();
    }

}


