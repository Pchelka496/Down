using Creatures.Player;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Additional;
using System.Runtime.CompilerServices;

public class PlayerStagnationDetector : IDisposable
{
    Rigidbody2D _playerRb;

    float _stagnationDurationThreshold;
    float _checkInterval;
    float _eventCooldown;
    float _acceptableThresholdXVelocity;

    float _failedCheckDuration;
    bool _eventOnCooldown;

    CancellationTokenSource _cts;

    event Action DisposeEvents;
    event Action OnStagnationDetected;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(PlayerController player, GlobalEventsManager globalEventsManager)
    {
        _playerRb = player.Rb;

        globalEventsManager.SubscribeToRoundStarted(OnRoundStart);
        globalEventsManager.SubscribeToRoundEnded(OnRoundEnd);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(OnRoundStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(OnRoundEnd);
    }

    public void Initialize(Initializer initializer)
    {
        _stagnationDurationThreshold = initializer.StagnationDurationThreshold;
        _checkInterval = initializer.CheckInterval;
        _eventCooldown = initializer.EventCooldown;
        _acceptableThresholdXVelocity = initializer.AcceptableThresholdXVelocity;
    }

    private void OnRoundStart()
    {
        ClearToken();
        _cts = new();

        MonitorPlayerMovementAsync(_cts.Token).Forget();
    }

    private void OnRoundEnd()
    {
        ClearToken();
    }

    private async UniTask MonitorPlayerMovementAsync(CancellationToken token)
    {
        var delaySpan = TimeSpan.FromSeconds(_checkInterval);

        while (!token.IsCancellationRequested)
        {
            await CheckPlayerMovement(token);
            await UniTask.Delay(delaySpan, cancellationToken: token);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async UniTask CheckPlayerMovement(CancellationToken token)
    {
        if (_eventOnCooldown) return;

        var velocity = _playerRb.velocity;

        if (IsÑorrectlyMovement(velocity) || IsMovingBack(velocity))
        {
            _failedCheckDuration += _checkInterval;
        }
        else
        {
            _failedCheckDuration = 0f;
            return;
        }

        if (_failedCheckDuration >= _stagnationDurationThreshold)
        {
            var cooldownSpan = TimeSpan.FromSeconds(_eventCooldown);

            _failedCheckDuration = 0f;
            _eventOnCooldown = true;

            TriggerStagnationEvent();

            await UniTask.Delay(cooldownSpan, cancellationToken: token);

            _eventOnCooldown = false;
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsÑorrectlyMovement(Vector2 velocity)
    {
        var absVelocity = new Vector2(Mathf.Abs(velocity.x), Mathf.Abs(velocity.y));

        if (_acceptableThresholdXVelocity >= absVelocity.x) return false;

        var isYVelocityMore = absVelocity.x < absVelocity.y;

        return !isYVelocityMore;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsMovingBack(Vector2 velocity)
    {
        return velocity.y > 0;
    }

    private void TriggerStagnationEvent()
    {
        OnStagnationDetected?.Invoke();
    }

    public void SubscribeToStagnationEvent(Action callback) => OnStagnationDetected += callback;
    public void UnsubscribeFromStagnationEvent(Action callback) => OnStagnationDetected -= callback;

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _cts);

    public void Dispose()
    {
        ClearToken();
        DisposeEvents?.Invoke();
    }

    [System.Serializable]
    public record Initializer
    {
        [SerializeField] int _stagnationDurationThreshold = 10;
        [SerializeField] float _checkInterval = 0.2f;
        [SerializeField] float _eventCooldown = 1f;
        [SerializeField] float _acceptableThresholdXVelocity = 5f;

        public int StagnationDurationThreshold { get => _stagnationDurationThreshold; }
        public float CheckInterval { get => _checkInterval; }
        public float EventCooldown { get => _eventCooldown; }
        public float AcceptableThresholdXVelocity { get => _acceptableThresholdXVelocity; }
    }
}
