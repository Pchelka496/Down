using System;

public class GameAnalytics : IDisposable
{
    readonly ICollectedMoneyTracker _moneyTracker;
    readonly IPlayerCollisionTracker _collisionTracker;
    readonly IRoundResultTracker _resultTracker;

    IAnalyticsManager _analyticsHelper;

    int _moneyCollected;
    int _collisionCount;
    RoundResult _roundResult;
    float _roundStartTime;
    float _roundEndTime;
    float? _playerStartHeight;
    float? _playerEndHeight;

    event Action DisposeEvents;
    public event Action<AnalyticsData> OnAnalyticsUpdated;

    public GameAnalytics(ICollectedMoneyTracker moneyTracker = null,
                         IPlayerCollisionTracker collisionTracker = null,
                         IRoundResultTracker resultTracker = null)
    {
        _moneyTracker = moneyTracker;
        _collisionTracker = collisionTracker;
        _resultTracker = resultTracker;

        SubscribeToEvents();
    }

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(IAnalyticsManager analyticsHelper)
    {
        _analyticsHelper = analyticsHelper;
    }

    private void SubscribeToEvents()
    {
        _moneyTracker?.SubscribeToMoneyChanged(OnMoneyChanged);
        _collisionTracker?.SubscribeToCollisionChanged(OnCollisionChanged);
        _resultTracker?.SubscribeToRoundResultChanged(OnRoundResultChanged);

        DisposeEvents += () => _collisionTracker?.UnsubscribeFromCollisionChanged(OnCollisionChanged);
        DisposeEvents += () => _resultTracker?.UnsubscribeFromRoundResultChanged(OnRoundResultChanged);
        DisposeEvents += () => _moneyTracker?.UnsubscribeFromMoneyChanged(OnMoneyChanged);
    }

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(Controls controls, GlobalEventsManager globalEventsManager)
    {
        controls.Enable();

        globalEventsManager.SubscribeToRoundStarted(RoundStart);
        globalEventsManager.SubscribeToRoundEnded(RoundEnd);
        globalEventsManager.SubscribeToFastTravelStarted(FastTravelStart);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
    }

    public void FastTravelStart()
    {
        _playerStartHeight = null;
        _playerEndHeight = null;
    }

    public void RoundStart()
    {
        _moneyCollected = 0;
        _collisionCount = 0;
        _roundResult = RoundResult.Unfinished;
        _roundStartTime = UnityEngine.Time.time;

        _playerStartHeight = PlayerPositionMeter.YPosition;
        _playerEndHeight = null;
    }

    public void RoundEnd()
    {
        _roundEndTime = UnityEngine.Time.time;
        _playerEndHeight = PlayerPositionMeter.YPosition;

        CollectAnalyticsData();

        _analyticsHelper.SendAnalyticsEvent(GetLastUpdatedAnalyticsData());
    }

    private void OnMoneyChanged(int money)
    {
        _moneyCollected = money;
        UpdateAnalytics();
    }

    private void OnCollisionChanged(int collisions)
    {
        _collisionCount = collisions;
        UpdateAnalytics();
    }

    private void OnRoundResultChanged(RoundResult result)
    {
        _roundResult = result;
        UpdateAnalytics();
    }

    private void UpdateAnalytics()
    {
        CollectAnalyticsData();
    }

    public AnalyticsData GetLastUpdatedAnalyticsData()
    {
        return new AnalyticsData(totalMoneyCollected: _moneyCollected,
                                 totalCollisions: _collisionCount,
                                 result: _roundResult,
                                 roundDuration: _roundEndTime - _roundStartTime,
                                 playerStartHeight: _playerStartHeight,
                                 playerEndHeight: _playerEndHeight
                                 );
    }

    private void CollectAnalyticsData()
    {
        if (OnAnalyticsUpdated == null) return;

        var analyticsData = GetLastUpdatedAnalyticsData();

        OnAnalyticsUpdated?.Invoke(analyticsData);
    }

    public void Dispose()
    {
        DisposeEvents?.Invoke();
    }
}
