using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class MapController : MonoBehaviour
{
    LevelManager _levelManager;
    MapControllerConfig _config;

    CheckpointPlatformController _checkpointPlatformController;

    public float FullMapHeight { get => LevelManager.PLAYER_START_Y_POSITION; }

    [Inject]
    private void Construct(LevelManager levelManager)
    {
        _levelManager = levelManager;
        levelManager.SubscribeToRoundStart(RoundStart);

        _checkpointPlatformController = GameplaySceneInstaller.DiContainer.Instantiate<CheckpointPlatformController>();
    }

    public void Initialize(MapControllerConfig config)
    {
        _config = config;

        _checkpointPlatformController.Initialize(this, config);

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
