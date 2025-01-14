using Additional;
using Creatures.Player;
using Cysharp.Threading.Tasks;
using Nenn.InspectorEnhancements.Runtime.Attributes;
using System;
using System.Threading;
using UnityEngine;

public class PlayerLaserPusher : MonoBehaviour
{
    [HideLabel][Required][SerializeField] LineRenderer _lineRenderer;
    [SerializeField] float _laserWidth = 1f;
    [SerializeField][Required] ParticleSystem _damageEffect;
    [Header("Laser damage setting")]
    [SerializeField] int _damage = 1;
    [SerializeField] float _damageCooldown = 1f;
    [SerializeField] float _damageCheckInterval = 0.02f;
    [Header("Laser speed setting")]
    [SerializeField] float _startSpeed = 1f;
    [SerializeField] float _timeTooSpeedReduction = 2f;
    [SerializeField] int _reducatuionSpeedCheckNumber = 60;

    HealthModule _healthModule;
    Rigidbody2D _playerRb;
    float _currentSpeed;
    float _currentLaserYPosition;
    float _laserToPlayerDistance;
    bool _isActive;
    bool _isVisible;

    CancellationTokenSource _cts;

    event Action OnDispose;

    public bool IsActive
    {
        set
        {
            _isActive = value;
            gameObject.SetActive(value);
        }
    }

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(
        PlayerController player,
        PlayerStagnationDetector stagnationDetector,
        GlobalEventsManager eventsManager,
        CameraFacade cameraFacade)
    {
        _healthModule = player.HealthModule;
        _playerRb = player.Rb;
        _laserToPlayerDistance = cameraFacade.GameplayMaxOrthographicSize;

        eventsManager.SubscribeToRoundStarted(OnRoundStart);
        eventsManager.SubscribeToRoundEnded(OnRoundEnd);

        OnDispose += () => eventsManager.UnsubscribeFromRoundStarted(OnRoundStart);
        OnDispose += () => eventsManager.UnsubscribeFromRoundEnded(OnRoundEnd);
        OnDispose += () => stagnationDetector.UnsubscribeFromStagnationEvent(HandleStagnation);

        stagnationDetector.SubscribeToStagnationEvent(HandleStagnation);

        OnRoundEnd();
    }

    private void Start()
    {
        ConfigureLineRenderer();
    }

    private void ConfigureLineRenderer()
    {
        _lineRenderer.startWidth = _laserWidth;
        _lineRenderer.endWidth = _laserWidth;
    }

    private void OnRoundStart()
    {
        IsActive = false;
    }

    private void OnRoundEnd()
    {
        StopPusher();
    }

    private void HandleStagnation()
    {
        if (_isActive) return;

        IsActive = true;

        ClearToken();
        _cts = new();

        UpdateLaserSpeedLoop(_cts.Token).Forget();
        UpdateLaserPositionLoop(_cts.Token).Forget();
        CheckPlayerUnderLaserLoop(_cts.Token).Forget();
    }

    private void OnBecameVisible()
    {
        _isVisible = true;
    }

    private void OnBecameInvisible()
    {
        StopPusher();
        _isVisible = false;
    }

    private void StopPusher()
    {
        ClearToken();
        IsActive = false;
    }

    private async UniTaskVoid UpdateLaserSpeedLoop(CancellationToken token)
    {
        _currentSpeed = _startSpeed;

        while (!token.IsCancellationRequested)
        {
            var absPlayerVelocity = new Vector2(Mathf.Abs(_playerRb.velocity.x), Mathf.Abs(_playerRb.velocity.y));

            if (absPlayerVelocity.x > absPlayerVelocity.y || _playerRb.velocity.y > 0)
            {
                const float INCREESE_SPEED_VALUE = 0.001f;
                _currentSpeed += INCREESE_SPEED_VALUE;
            }
            else
            {
                if (await MaintainCorrectMovement(_timeTooSpeedReduction, _reducatuionSpeedCheckNumber, token))
                {
                    await ReduceSpeed(token);
                }
            }

            await UniTask.WaitForFixedUpdate(cancellationToken: token);
        }
    }

    private async UniTask ReduceSpeed(CancellationToken token)
    {
        while (_currentSpeed > 0)
        {
            const float REDUCE_SPEED_VALUE = 0.05f;

            _currentSpeed -= REDUCE_SPEED_VALUE;
            _currentSpeed = Mathf.Max(_currentSpeed, 0f);

            await UniTask.WaitForFixedUpdate(cancellationToken: token);

            var absPlayerVelocity = new Vector2(Mathf.Abs(_playerRb.velocity.x), Mathf.Abs(_playerRb.velocity.y));

            if (absPlayerVelocity.x > absPlayerVelocity.y)
            {
                return;
            }
        }

        if (!_isVisible)
        {
            StopPusher();
        }
    }

    private async UniTask<bool> MaintainCorrectMovement(float duration, int checks, CancellationToken token)
    {
        var elapsed = 0f;
        var checkInterval = duration / checks;

        while (elapsed < duration)
        {
            var absPlayerVelocity = new Vector2(Mathf.Abs(_playerRb.velocity.x), Mathf.Abs(_playerRb.velocity.y));

            if (absPlayerVelocity.x > absPlayerVelocity.y)
            {
                return false;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(checkInterval), cancellationToken: token);
            elapsed += checkInterval;
        }

        return true;
    }

    private async UniTaskVoid UpdateLaserPositionLoop(CancellationToken token)
    {
        const float StartPositionOffset = 100f;

        _currentLaserYPosition = PlayerPositionMeter.YPosition + StartPositionOffset;

        while (!token.IsCancellationRequested)
        {
            var playerYPosition = PlayerPositionMeter.YPosition;
            var direction = Mathf.Sign(playerYPosition - _currentLaserYPosition);
            var deltaY = _currentSpeed * direction;

            _currentLaserYPosition = Mathf.MoveTowards(_currentLaserYPosition, playerYPosition, Mathf.Abs(deltaY));

            UpdateLaserPosition();
            await UniTask.WaitForFixedUpdate(cancellationToken: token);
        }
    }

    private async UniTaskVoid CheckPlayerUnderLaserLoop(CancellationToken token)
    {
        var damageCooldown = TimeSpan.FromSeconds(_damageCooldown);
        var checkInterval = TimeSpan.FromSeconds(_damageCheckInterval);

        while (!token.IsCancellationRequested)
        {
            if (IsPlayerUnderLaser())
            {
                _healthModule.ApplyDamage(_damage);
                _damageEffect.Play();

                await UniTask.Delay(damageCooldown, cancellationToken: token);
            }

            await UniTask.Delay(checkInterval, cancellationToken: token);
        }
    }

    private void UpdateLaserPosition()
    {
        var leftPoint = new Vector3(
            x: PlayerPositionMeter.XPosition - _laserToPlayerDistance,
            y: _currentLaserYPosition);

        var rightPoint = new Vector3(
            x: PlayerPositionMeter.XPosition + _laserToPlayerDistance,
            y: _currentLaserYPosition);

        _lineRenderer.SetPosition(0, leftPoint);
        _lineRenderer.SetPosition(1, rightPoint);
    }

    private bool IsPlayerUnderLaser()
    {
        var playerPosition = _healthModule.transform.position;
        var laserYPosition = _lineRenderer.GetPosition(0).y;

        return Mathf.Abs(playerPosition.y - laserYPosition) < _laserWidth;
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _cts);

    private void OnDestroy()
    {
        OnDispose?.Invoke();
        ClearToken();
    }
}
