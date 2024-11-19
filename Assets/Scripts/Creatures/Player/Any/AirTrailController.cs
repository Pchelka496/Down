using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class AirTrailController : MonoBehaviour
{
    const float Z_ROTATION_OFFSET = 0f;

    [SerializeField] Material _airTrailMaterial;
    [SerializeField] SpriteRenderer _spriteRenderer;

    [SerializeField] float _minSpeedValue = 5f;
    [SerializeField] float _maxSpeedValue = 10f;

    [SerializeField] float _minAlpha = 0.2f;
    [SerializeField] float _maxAlpha = 1f;

    [Header("AirBrakeEnabled")]
    [SerializeField] Vector3 _defaultScale = Vector3.one;
    [SerializeField] float _minDefaultModeVelocityThreshold = -40f;
    [SerializeField] float _maxDefaultModeVelocityThreshold = -20f;

    [Header("AirBrakeEnabled")]
    [SerializeField] Vector3 _airBrakeScale = Vector3.one;
    [SerializeField] float _minAirBrakeModeVelocityThreshold = -40f;
    [SerializeField] float _maxAirBrakeModeVelocityThreshold = -20f;

    float _minVelocityThreshold = -40f;
    float _maxVelocityThreshold = -20f;

    bool _isAirBrakeActive = false;
    Rigidbody2D _rb;
    CancellationTokenSource _cts;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(CharacterController player)
    {
        _rb = player.Rb;
        player.MultiTargetRotationFollower.RegisterRotationObject(transform, Z_ROTATION_OFFSET);
        AirBrakeDisabled();
        transform.rotation = Quaternion.Euler(0f, 0f, Z_ROTATION_OFFSET);
    }

    private void OnEnable()
    {
        StartAdjustingTrailAsync().Forget();
    }

    private void OnDisable()
    {
        _cts?.Cancel();
    }

    private async UniTaskVoid StartAdjustingTrailAsync()
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        while (!token.IsCancellationRequested)
        {
            if (_rb.velocity.y > _minVelocityThreshold)
            {
                _spriteRenderer.enabled = false;
                await UniTask.WaitUntil(() => _rb.velocity.y <= _minVelocityThreshold);
                _spriteRenderer.enabled = true;
            }

            var velocityY = _rb.velocity.y;

            var normalizedSpeed = Mathf.InverseLerp(_minVelocityThreshold, _maxVelocityThreshold, velocityY);
            var targetSpeedValue = Mathf.Lerp(_minSpeedValue, _maxSpeedValue, normalizedSpeed);
            _airTrailMaterial.SetFloat("Speed", targetSpeedValue);

            var normalizedAlpha = Mathf.InverseLerp(_minVelocityThreshold, _maxVelocityThreshold, velocityY);
            var targetAlpha = Mathf.Lerp(_minAlpha, _maxAlpha, normalizedAlpha);

            var spriteColor = _spriteRenderer.color;
            spriteColor.a = targetAlpha;
            _spriteRenderer.color = spriteColor;

            await UniTask.Yield(token);
        }
    }

    public void SetAirBrakeStatus(bool isAirBrakeActive)
    {
        _isAirBrakeActive = isAirBrakeActive;

        if (_isAirBrakeActive)
        {
            AirBrakeEnabled();
        }
        else
        {
            AirBrakeDisabled();

        }
    }

    public void SwitchAirBrake(InputAction.CallbackContext ctx)
    {
        if (_isAirBrakeActive)
        {
            AirBrakeDisabled();
            _isAirBrakeActive = false;
        }
        else
        {
            AirBrakeEnabled();
            _isAirBrakeActive = true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AirBrakeEnabled()
    {
        transform.localScale = _airBrakeScale;
        _minVelocityThreshold = _minAirBrakeModeVelocityThreshold;
        _maxVelocityThreshold = _maxAirBrakeModeVelocityThreshold;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AirBrakeDisabled()
    {
        transform.localScale = _defaultScale;
        _minVelocityThreshold = _minDefaultModeVelocityThreshold;
        _maxVelocityThreshold = _maxDefaultModeVelocityThreshold;
    }

}
