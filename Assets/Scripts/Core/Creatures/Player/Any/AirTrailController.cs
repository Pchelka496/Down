using System.Runtime.CompilerServices;
using System.Threading;
using Additional;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Creatures.Player.Any
{
    public class AirTrailController : MonoBehaviour
    {
        const float Z_ROTATION_OFFSET = 0f;
        static readonly int speed = Shader.PropertyToID("Speed");

        [SerializeField] Material _airTrailMaterial;
        [SerializeField] SpriteRenderer _spriteRenderer;

        [SerializeField] float _minSpeedValue = 5f;
        [SerializeField] float _maxSpeedValue = 10f;

        [SerializeField] float _minAlpha = 0.2f;
        [SerializeField] float _maxAlpha = 1f;

        [Header("AirBrakeEnabled")]
        [SerializeField]
        Vector3 _defaultScale = Vector3.one;

        [SerializeField] float _minDefaultModeVelocityThreshold = -40f;
        [SerializeField] float _maxDefaultModeVelocityThreshold = -20f;

        [Header("AirBrakeEnabled")]
        [SerializeField]
        Vector3 _airBrakeScale = Vector3.one;

        [SerializeField] float _minAirBrakeModeVelocityThreshold = -40f;
        [SerializeField] float _maxAirBrakeModeVelocityThreshold = -20f;

        float _minVelocityThreshold = -40f;
        float _maxVelocityThreshold = -20f;

        bool _isAirBrakeActive;
        Rigidbody2D _rb;
        CancellationTokenSource _cts;

        [Inject]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
        private void Construct(PlayerController player)
        {
            _rb = player.Rb;
            player.MultiTargetRotationFollower.RegisterRotationObject(transform);
            AirBrakeDisabled();
            transform.rotation = Quaternion.Euler(0f, 0f, Z_ROTATION_OFFSET);
        }

        private void OnEnable()
        {
            ClearToken();
            _cts = new();
            StartAdjustingTrailAsync(_cts.Token).Forget();
        }

        private void OnDisable()
        {
            ClearToken();
        }

        private async UniTaskVoid StartAdjustingTrailAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_rb.velocity.y > _minVelocityThreshold)
                {
                    _spriteRenderer.enabled = false;
                    // ReSharper disable once MethodSupportsCancellation
                    await UniTask.WaitUntil(() => _rb.velocity.y <= _minVelocityThreshold);
                    _spriteRenderer.enabled = true;
                }

                var velocityY = _rb.velocity.y;

                var normalizedSpeed = Mathf.InverseLerp(_minVelocityThreshold, _maxVelocityThreshold, velocityY);
                var targetSpeedValue = Mathf.Lerp(_minSpeedValue, _maxSpeedValue, normalizedSpeed);
                _airTrailMaterial.SetFloat(speed, targetSpeedValue);

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

        private void ClearToken() => ClearTokenSupport.ClearToken(ref _cts);

        private void OnDestroy()
        {
            ClearToken();
        }
    }
}