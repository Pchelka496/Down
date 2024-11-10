using UnityEngine;
using Zenject;
using DG.Tweening;

public class AirBrakeModule : BaseModule
{
    const float Z_ROTATION_OFFSET = 90f;

    [SerializeField] Transform _leftAirBrake;
    [SerializeField] Transform _rightAirBrake;

    [SerializeField] float _defaultDrag = 0.2f;
    float _airBrakeDrag = 10f;

    [SerializeField] Vector3 _leftAirBrakeOpenPosition;
    [SerializeField] Vector3 _leftAirBrakeClosePosition;
    [SerializeField] Vector3 _rightAirBrakeOpenPosition;
    [SerializeField] Vector3 _rightAirBrakeClosePosition;

    float _airBrakeReleaseRate = 0.5f;

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

    private void UpdateCharacteristics() => UpdateCharacteristics(GameplaySceneInstaller.DiContainer.Resolve<AirBrakeModuleConfig>());

    private void UpdateCharacteristics(AirBrakeModuleConfig config)
    {
        _airBrakeDrag = config.AirMaxBrakeDrag;
        _airBrakeReleaseRate = config.AirBrakeReleaseRate;
    }

    private void SwitchAirBrake()
    {
        _currentTween?.Kill();

        var sequence = DOTween.Sequence();

        if (_isAirBrakeActive)
        {
            sequence.Append(DOTween.To(() => _rightAirBrake.localPosition,
                                       x => _rightAirBrake.localPosition = x,
                                       _rightAirBrakeClosePosition,
                                       _airBrakeReleaseRate).SetEase(Ease.OutQuad));

            sequence.Join(DOTween.To(() => _leftAirBrake.localPosition,
                                     x => _leftAirBrake.localPosition = x,
                                     _leftAirBrakeClosePosition,
                                     _airBrakeReleaseRate).SetEase(Ease.OutQuad));

            sequence.Join(DOTween.To(() => _rb.drag,
                                     x => _rb.drag = x,
                                     _defaultDrag,
                                     _airBrakeReleaseRate).SetEase(Ease.OutQuad));

            _isAirBrakeActive = false;
        }
        else
        {
            sequence.Append(DOTween.To(() => _rightAirBrake.localPosition,
                                       x => _rightAirBrake.localPosition = x,
                                       _rightAirBrakeOpenPosition,
                                       _airBrakeReleaseRate).SetEase(Ease.OutQuad));

            sequence.Join(DOTween.To(() => _leftAirBrake.localPosition,
                                     x => _leftAirBrake.localPosition = x,
                                     _leftAirBrakeOpenPosition,
                                     _airBrakeReleaseRate).SetEase(Ease.OutQuad));

            sequence.Join(DOTween.To(() => _rb.drag,
                                     x => _rb.drag = x,
                                     _airBrakeDrag,
                                     _airBrakeReleaseRate).SetEase(Ease.OutQuad));

            _isAirBrakeActive = true;
        }

        _currentTween = sequence;
    }

    private void OnDestroy()
    {
        GameplaySceneInstaller.DiContainer.Resolve<Controls>().Player.AirBreake.performed -= ctx => SwitchAirBrake();
        GameplaySceneInstaller.DiContainer.Resolve<Controls>().Player.AirBreake.performed -= ctx => _airTrailController.SwitchAirBrake();
    }

}
