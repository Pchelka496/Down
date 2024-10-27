using UnityEngine;
using Zenject;

public class MapController : MonoBehaviour
{
    LevelManager _levelManager;
    MapControllerConfig _config;

    CheckpointPlatformController _checkpointPlatform;

    public float FullMapHeight { get => LevelManager.PLAYER_START_Y_POSITION; }

    [Inject]
    private void Construct(LevelManager levelManager)
    {
        _levelManager = levelManager;
        levelManager.SubscribeToRoundStart(RoundStart);

        _checkpointPlatform = GameplaySceneInstaller.DiContainer.Instantiate<CheckpointPlatformController>();
    }

    public void Initialize(MapControllerConfig config)
    {
        _config = config;

        _checkpointPlatform.Initialize(this, config);

        var level = LevelManager.PLAYER_START_Y_POSITION;

        _ = _checkpointPlatform.CreatePlatforms(level);
    }

    private void RoundStart(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundEnd(RoundEnd);

    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);

    }

}
