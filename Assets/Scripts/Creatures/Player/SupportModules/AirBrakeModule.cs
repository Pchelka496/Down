using UnityEngine;
using Zenject;
using DG.Tweening;

public class AirBrakeModule : BaseModule
{
    [SerializeField] Transform _leftAirBrake;
    [SerializeField] Transform _rightAirBrake;

    [SerializeField] float _airBrakeDrag = 10f;
    [SerializeField] float _defaultDrag = 0.2f;

    [SerializeField] Vector2 _leftAirBrakeOpenPosition;
    [SerializeField] Vector2 _leftAirBrakeClosePosition;
    [SerializeField] Vector2 _rightAirBrakeOpenPosition;
    [SerializeField] Vector2 _rightAirBrakeClosePosition;

    [SerializeField] float _transitionTime = 0.5f;

    Rigidbody2D _rb;
    bool _isAirBrakeActive = false;
    Tween _currentTween;

    [Inject]
    private void Construct(AirBrakeModuleConfig config, CharacterController player, Controls controls)
    {
        _rb = player.Rb;

        transform.SetParent(player.transform, false);

        _leftAirBrake.localPosition = _leftAirBrakeClosePosition;
        _rightAirBrake.localPosition = _rightAirBrakeClosePosition;

        controls.Player.AirBreake.performed += ctx => SwitchAirBrake();
    }

    private void SwitchAirBrake()
    {
        _currentTween?.Kill();

        if (_isAirBrakeActive)
        {
            _currentTween = DOTween.To(() => (Vector2)_rightAirBrake.localPosition,
                                        x => _rightAirBrake.localPosition = x,
                                        _rightAirBrakeClosePosition,
                                        _transitionTime).SetEase(Ease.OutQuad);

            _currentTween = DOTween.To(() => (Vector2)_leftAirBrake.localPosition,
                                       x => _leftAirBrake.localPosition = x,
                                       _leftAirBrakeClosePosition,
                                       _transitionTime).SetEase(Ease.OutQuad);

            _rb.drag = _defaultDrag;
            _isAirBrakeActive = false;
        }
        else
        {
            _currentTween = DOTween.To(() => (Vector2)_rightAirBrake.localPosition,
                                        x => _rightAirBrake.localPosition = x,
                                        _rightAirBrakeOpenPosition,
                                        _transitionTime).SetEase(Ease.OutQuad);

            _currentTween = DOTween.To(() => (Vector2)_leftAirBrake.localPosition,
                                       x => _leftAirBrake.localPosition = x,
                                       _leftAirBrakeOpenPosition,
                                       _transitionTime).SetEase(Ease.OutQuad);
            _rb.drag = _airBrakeDrag;
            _isAirBrakeActive = true;
        }
    }

    private void OnDestroy()
    {
        GameplaySceneInstaller.DiContainer.Resolve<Controls>().Player.AirBreake.performed += ctx => SwitchAirBrake();
    }

}
