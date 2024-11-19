using UnityEngine;
using Zenject;

public class PickUpItemManager : MonoBehaviour
{
    public const int REWARD_LAYER_INDEX = 11;
    PickUpItemManagerConfig _config;
    RewardController _rewardController;
    RepairKitController _repairKitController;

    [Inject]
    private void Construct(LevelManager levelManager, CharacterController player, RewardCounter rewardCounter)
    {
        Reward.Initialize(player, rewardCounter);

        _rewardController = GameplaySceneInstaller.DiContainer.Instantiate<RewardController>();
        _repairKitController = GameplaySceneInstaller.DiContainer.Instantiate<RepairKitController>();
    }

    public void Initialize(PickUpItemManagerConfig config)
    {
        _config = config;
        _rewardController.Initialize(config.RewardControllerConfig, transform);
        _repairKitController.Initialize(config.RepairKitControllerConfig, transform);
    }

    public int GetPoints() => _config.GetPoints();

    public void IncreasePoints(int increaseValue) => _config.IncreasePoints(increaseValue);

    public void DecreasePoints(int decreaseValue) => _config.DecreasePoints(decreaseValue);

}
