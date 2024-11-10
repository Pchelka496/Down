using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class RewardManager : MonoBehaviour
{
    public const int REWARD_LAYER_INDEX = 11;
    RewardManagerConfig _config;
    RewardController _controller;

    [Inject]
    private void Construct(LevelManager levelManager, CharacterController player, RewardCounter rewardCounter)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
        Reward.Initialize(player, this, rewardCounter);

        _controller = GameplaySceneInstaller.DiContainer.Instantiate<RewardController>();
    }

    public void Initialize(RewardManagerConfig config)
    {
        _config = config;
        _controller.Initialize(config.RewardControllerConfig, transform);
    }

    private void RoundStart(LevelManager levelManager)
    {

    }

    public int GetPoints() => _config.GetPoints();

    public void IncreasePoints(int increaseValue) => _config.IncreasePoints(increaseValue);

    public void DecreasePoints(int decreaseValue) => _config.DecreasePoints(decreaseValue);


}
