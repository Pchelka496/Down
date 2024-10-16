using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using static MapControllerConfig;

public class MapController : MonoBehaviour
{
    LevelManager _levelManager;
    MapControllerConfig _config;

    CheckpointPlatformController _checkpointPlatform;
    RewardController _rewardController;

    public float FirstHeight => _config.FirstPlatformHeight();
    public float FullMapHeight { get => _config.MaximumHeight; }

    [Inject]
    private void Construct(LevelManager levelManager)
    {
        _levelManager = levelManager;

        _checkpointPlatform = GameplaySceneInstaller.DiContainer.Instantiate<CheckpointPlatformController>();
        _rewardController = GameplaySceneInstaller.DiContainer.Instantiate<RewardController>();
    }

    public void Initialize(MapControllerConfig config)
    {
        _config = config;

        _checkpointPlatform.Initialize(this, config);
        _rewardController.Initialize(this, config.RewardControllerConfig);

        var level = _config.GetSavingHeight(_levelManager.PlayerSavedHeight);

        _ = _checkpointPlatform.CreatePlatforms(level);
    }

    public PlatformInformation GetClosestPlatformAboveHeight(float height) => _config.GetClosestPlatformAboveHeight(height);
    public PlatformInformation GetClosestPlatformBelowHeight(float height) => _config.GetClosestPlatformBelowHeight(height);

}
