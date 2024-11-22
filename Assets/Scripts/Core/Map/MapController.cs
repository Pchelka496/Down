using Cysharp.Threading.Tasks;
using Zenject;

public class MapController
{
    CheckpointPlatformController _checkpointPlatformController;

    public float FullMapHeight { get => LevelManager.PLAYER_START_Y_POSITION; }

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundStart(RoundStart);

        _checkpointPlatformController = GameplaySceneInstaller.DiContainer.Instantiate<CheckpointPlatformController>();
    }

    public void Initialize(MapControllerConfig config)
    {
        _checkpointPlatformController.Initialize(config);

        _checkpointPlatformController.CreatePlatforms(LevelManager.PLAYER_START_Y_POSITION).Forget();
    }

    private void RoundStart(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundEnd(RoundEnd);
        _checkpointPlatformController.ClearPlatform();
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
        _checkpointPlatformController.CreatePlatforms(LevelManager.PLAYER_START_Y_POSITION).Forget();
    }

}
