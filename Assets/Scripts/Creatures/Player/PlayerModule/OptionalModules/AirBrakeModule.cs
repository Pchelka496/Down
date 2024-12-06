using UnityEngine;
using Zenject;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Runtime.CompilerServices;
using Core.Installers;
using Creatures.Player;
using Creatures.Player.Any;
using ScriptableObject.ModulesConfig.SupportModules;

public class AirBrakeModule : BaseModule
{
    const float Z_ROTATION_OFFSET = 90f;
    const float DEFAULT_DRAG = 0.2f;

    [SerializeField] Transform _leftAirBrake;
    [SerializeField] Transform _rightAirBrake;

    [SerializeField] Vector3 _leftAirBrakeOpenPosition;
    [SerializeField] Vector3 _leftAirBrakeClosePosition;
    [SerializeField] Vector3 _rightAirBrakeOpenPosition;
    [SerializeField] Vector3 _rightAirBrakeClosePosition;

    [SerializeField] float _airBrakeDrag = 10f;
    [SerializeField] float _airBrakeReleaseRate = 0.5f;

    Controls _controls;
    AirTrailController _airTrailController;
    Rigidbody2D _rb;
    bool _isAirBrakeActive = false;
    Tween _currentTween;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:������� �������������� �������� �����", Justification = "<��������>")]
    private void Construct(AirBrakeModuleConfig config, PlayerController player, Controls controls, AirTrailController airTrailController)
    {
        _rb = player.Rb;

        SnapToPlayer(player.transform);

        player.MultiTargetRotationFollower.RegisterRotationObject(transform, Z_ROTATION_OFFSET);

        _leftAirBrake.localPosition = _leftAirBrakeClosePosition;
        _rightAirBrake.localPosition = _rightAirBrakeClosePosition;
        _airTrailController = airTrailController;
        _controls = controls;

        UpdateCharacteristics(config);
    }

    public override void EnableModule()
    {
        _controls.Player.AirBreake.performed += SwitchAirBrake;
        _controls.Player.AirBreake.performed += _airTrailController.SwitchAirBrake;

        _airTrailController.SetAirBrakeStatus(false);
    }

    public override void DisableModule()
    {
        _controls.Player.AirBreake.performed -= SwitchAirBrake;
        _controls.Player.AirBreake.performed -= _airTrailController.SwitchAirBrake;

        _airTrailController.SetAirBrakeStatus(false);
    }

    public void UpdateCharacteristics() => UpdateCharacteristics(GameplaySceneInstaller.DiContainer.Resolve<AirBrakeModuleConfig>());

    public void UpdateCharacteristics(AirBrakeModuleConfig config)
    {
        _airBrakeDrag = config.AirMaxBrakeDrag;
        _airBrakeReleaseRate = config.AirBrakeReleaseRate;
    }

    private void SwitchAirBrake(InputAction.CallbackContext ctx)
    {
        _currentTween?.Kill();

        var sequence = DOTween.Sequence();

        if (_isAirBrakeActive)
        {
            AirBrakeDisabled(sequence);
            _isAirBrakeActive = false;
        }
        else
        {
            AirBrakeEnabled(sequence);
            _isAirBrakeActive = true;
        }

        _currentTween = sequence;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AirBrakeEnabled(DG.Tweening.Sequence sequence)
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AirBrakeDisabled(DG.Tweening.Sequence sequence)
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
                                 DEFAULT_DRAG,
                                 _airBrakeReleaseRate).SetEase(Ease.OutQuad));
    }

    private void OnDestroy()
    {
        DisableModule();
        _airTrailController.SetAirBrakeStatus(false);
        GameplaySceneInstaller.DiContainer.Resolve<PlayerController>().MultiTargetRotationFollower.UnregisterRotationObject(transform);
    }

}
