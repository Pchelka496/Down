using Additional;
using Core;
using Core.Installers;
using Creatures.Player;
using Cysharp.Threading.Tasks;
using ScriptableObject.Map;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

public class MapController : System.IDisposable
{
    const float CREATE_SURFACE_THRESHOLD = 2000f;
    const float SURFACE_CHACKING_LOOP_DALEY = 1f;

    readonly AssetReference _surfaceController;
    CheckpointPlatformController _checkpointPlatformController;
    CancellationTokenSource _surfaceCheckCts;

    public MapController(AssetReference surfaceController)
    {
        _surfaceController = surfaceController;
    }

    event System.Action DisposeEvents;

    public float FullMapHeight => PlayerController.PLAYER_START_Y_POSITION;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(GlobalEventsManager globalEventsManager)
    {
        globalEventsManager.SubscribeToRoundStarted(RoundStart);
        globalEventsManager.SubscribeToRoundEnded(RoundEnd);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);

        _checkpointPlatformController = GameplaySceneInstaller.DiContainer.Instantiate<CheckpointPlatformController>();
    }

    public void Initialize(MapControllerConfig config)
    {
        _checkpointPlatformController.Initialize(config);

        _checkpointPlatformController.CreatePlatforms(PlayerController.PLAYER_START_Y_POSITION).Forget();
    }

    private void RoundStart()
    {
        _checkpointPlatformController.ClearPlatform();

        ClearToken();
        _surfaceCheckCts = new();

        SurfaceCheckingLoop(_surfaceCheckCts.Token).Forget();
    }

    private void RoundEnd()
    {
        ClearToken();
        _checkpointPlatformController.CreatePlatforms(PlayerController.PLAYER_START_Y_POSITION).Forget();
    }

    private async UniTaskVoid SurfaceCheckingLoop(CancellationToken token)
    {
        var spanDaley = System.TimeSpan.FromSeconds(SURFACE_CHACKING_LOOP_DALEY);

        while (true)
        {
            await UniTask.Delay(spanDaley, cancellationToken: token);

            if (CREATE_SURFACE_THRESHOLD >= PlayerPositionMeter.YPosition)
            {
                CreateSurfaceCreator().Forget();
                return;
            }
        }
    }

    private async UniTask CreateSurfaceCreator()
    {
        var loadData = await AddressableLouderHelper.LoadAssetAsync<GameObject>(_surfaceController);
        GameplaySceneInstaller.DiContainer.InstantiatePrefab(loadData.LoadAsset);
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _surfaceCheckCts);

    public void Dispose()
    {
        ClearToken();
        DisposeEvents?.Invoke();
    }
}
