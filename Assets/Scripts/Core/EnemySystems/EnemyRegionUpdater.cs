using Additional;
using Cysharp.Threading.Tasks;
using ScriptableObject.Enemy;
using System.Threading;

public class EnemyRegionUpdater : System.IDisposable
{
    EnemyManagerConfig _config;

    CancellationTokenSource _enemyRegionUpdaterCts;

    System.Action<EnemyRegionConfig> _updateRegion;
    event System.Action DisposeEvents;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(GlobalEventsManager globalEventsManager)
    {
        globalEventsManager.SubscribeToRoundStarted(RoundStart);
        globalEventsManager.SubscribeToRoundEnded(RoundEnd);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
    }

    public void Initialize(EnemyManagerConfig config, System.Action<EnemyRegionConfig> updateRegion)
    {
        _config = config;
        _updateRegion = updateRegion;
    }

    private void RoundStart()
    {
        ClearToken(ref _enemyRegionUpdaterCts);
        _enemyRegionUpdaterCts = new();

        UpdateRegionLoop(_enemyRegionUpdaterCts.Token).Forget();
    }

    private void RoundEnd()
    {
        ClearToken(ref _enemyRegionUpdaterCts);
    }

    private async UniTask UpdateRegionLoop(CancellationToken token)
    {
        var enemyRegions = _config.EnemyRegions;

        while (enemyRegions.Count > 0 && !token.IsCancellationRequested)
        {
            var currentRegion = enemyRegions.Pop();

            await UniTask.WaitUntil(
                () => PlayerPositionMeter.YPosition <= currentRegion.StartHeight, cancellationToken: token);

            if (enemyRegions.Count > 0)
            {
                var nextRegion = enemyRegions.Peek();

                if (PlayerPositionMeter.YPosition < nextRegion.StartHeight)
                {
                    continue;
                }
            }

            _updateRegion?.Invoke(currentRegion);
        }
    }

    private void ClearToken(ref CancellationTokenSource cts) => ClearTokenSupport.ClearToken(ref cts);

    public void Dispose()
    {
        ClearToken(ref _enemyRegionUpdaterCts);
        DisposeEvents?.Invoke();
    }
}
