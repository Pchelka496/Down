using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using Zenject;

public class AirTrailController : MonoBehaviour
{
    const float ROTAION_Z = -90f;

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
    private void Construct(CharacterController player, LevelManager levelManager, Controls controls)
    {
        _rb = player.Rb;
        AirBrakeDisabled();
        controls.Player.AirBreake.performed += ctx => SwitchAirBrake();
        transform.rotation = Quaternion.Euler(0f, 0f, ROTAION_Z);

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

            float velocityY = _rb.velocity.y;

            float normalizedSpeed = Mathf.InverseLerp(_minVelocityThreshold, _maxVelocityThreshold, velocityY);
            float targetSpeedValue = Mathf.Lerp(_minSpeedValue, _maxSpeedValue, normalizedSpeed);
            _airTrailMaterial.SetFloat("Speed", targetSpeedValue);

            float normalizedAlpha = Mathf.InverseLerp(_minVelocityThreshold, _maxVelocityThreshold, velocityY);
            float targetAlpha = Mathf.Lerp(_minAlpha, _maxAlpha, normalizedAlpha);

            Color spriteColor = _spriteRenderer.color;
            spriteColor.a = targetAlpha;
            _spriteRenderer.color = spriteColor;

            await UniTask.Yield(token);
        }
    }

    private void SwitchAirBrake()
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

    //public async UniTask ScaleObjectYAsync(CancellationToken token)
    //{
    //    // Увеличение масштаба по оси Y
    //    await transform.DOScaleY(_maxScaleY, _scaleTime)
    //                    .SetEase(Ease.OutQuad)
    //                    .AsyncWaitForCompletion();

    //    // Возвращение масштаба обратно к 1
    //    await transform.DOScaleY(1f, _scaleTime)
    //                    .SetEase(Ease.InQuad)
    //                    .AsyncWaitForCompletion();
    //}

    private void OnDestroy()
    {
        GameplaySceneInstaller.DiContainer.Resolve<Controls>().Player.AirBreake.performed -= ctx => SwitchAirBrake();

    }

}
