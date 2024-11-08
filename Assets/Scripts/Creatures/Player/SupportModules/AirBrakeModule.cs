using UnityEngine;
using Zenject;
using DG.Tweening;

public class AirBrakeModule : BaseModule
{
    const float Z_ROTATION_OFFSET = 90f;
    [SerializeField] Transform _leftAirBrake;
    [SerializeField] Transform _rightAirBrake;

    [SerializeField] float _airBrakeDrag = 10f;
    [SerializeField] float _defaultDrag = 0.2f;

    [SerializeField] Vector3 _leftAirBrakeOpenPosition;
    [SerializeField] Vector3 _leftAirBrakeClosePosition;
    [SerializeField] Vector3 _rightAirBrakeOpenPosition;
    [SerializeField] Vector3 _rightAirBrakeClosePosition;

    [SerializeField] float _transitionTime = 0.5f;

    Controls _controls;
    AirTrailController _airTrailController;
    Rigidbody2D _rb;
    bool _isAirBrakeActive = false;
    Tween _currentTween;

    [Inject]
    private void Construct(AirBrakeModuleConfig config, CharacterController player, Controls controls, AirTrailController airTrailController, LevelManager levelManager)
    {
        _rb = player.Rb;

        SnapToPlayer(player.transform);

        player.MultiTargetRotationFollower.RegisterRotationObject(transform, Z_ROTATION_OFFSET);

        _leftAirBrake.localPosition = _leftAirBrakeClosePosition;
        _rightAirBrake.localPosition = _rightAirBrakeClosePosition;
        _airTrailController = airTrailController;
        _controls = controls;

        levelManager.SubscribeToRoundStart(RoundStart);
    }

    private void RoundStart(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundEnd(RoundEnd);

        _controls.Player.AirBreake.performed += ctx => SwitchAirBrake();
        _controls.Player.AirBreake.performed += ctx => _airTrailController.SwitchAirBrake();
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);

        _controls.Player.AirBreake.performed -= ctx => SwitchAirBrake();
        _controls.Player.AirBreake.performed -= ctx => _airTrailController.SwitchAirBrake();
    }

    private void SwitchAirBrake()
    {
        _currentTween?.Kill();

        if (_isAirBrakeActive)
        {
            _currentTween = DOTween.To(() => _rightAirBrake.localPosition,
                                        x => _rightAirBrake.localPosition = x,
                                        _rightAirBrakeClosePosition,
                                        _transitionTime).SetEase(Ease.OutQuad);

            _currentTween = DOTween.To(() => _leftAirBrake.localPosition,
                                       x => _leftAirBrake.localPosition = x,
                                       _leftAirBrakeClosePosition,
                                       _transitionTime).SetEase(Ease.OutQuad);

            _rb.drag = _defaultDrag;
            _isAirBrakeActive = false;
        }
        else
        {
            _currentTween = DOTween.To(() => _rightAirBrake.localPosition,
                                        x => _rightAirBrake.localPosition = x,
                                        _rightAirBrakeOpenPosition,
                                        _transitionTime).SetEase(Ease.OutQuad);

            _currentTween = DOTween.To(() => _leftAirBrake.localPosition,
                                       x => _leftAirBrake.localPosition = x,
                                       _leftAirBrakeOpenPosition,
                                       _transitionTime).SetEase(Ease.OutQuad);
            _rb.drag = _airBrakeDrag;
            _isAirBrakeActive = true;
        }
    }

    private void OnDestroy()
    {
        GameplaySceneInstaller.DiContainer.Resolve<Controls>().Player.AirBreake.performed -= ctx => SwitchAirBrake();
        GameplaySceneInstaller.DiContainer.Resolve<Controls>().Player.AirBreake.performed -= ctx => _airTrailController.SwitchAirBrake();
    }

}
