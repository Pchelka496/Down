using Core;
using Core.Installers;
using Cysharp.Threading.Tasks;
using ScriptableObject.Map;
using Zenject;

public class MapController : System.IDisposable
{
    CheckpointPlatformController _checkpointPlatformController;

    event System.Action DisposeEvents;
    public float FullMapHeight => LevelManager.PLAYER_START_Y_POSITION;

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

        _checkpointPlatformController.CreatePlatforms(LevelManager.PLAYER_START_Y_POSITION).Forget();
    }

    private void RoundStart()
    {
        _checkpointPlatformController.ClearPlatform();
    }

    private void RoundEnd()
    {
        _checkpointPlatformController.CreatePlatforms(LevelManager.PLAYER_START_Y_POSITION).Forget();
    }

    public void Dispose()
    {
        DisposeEvents?.Invoke();
    }
}
