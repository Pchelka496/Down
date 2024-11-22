using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class RewardController
{
    const float MIN_DISTANCE_X = -30f;
    const float MAX_DISTANCE_X = 30f;
    const float MIN_DISTANCE_Y = 60f;
    const float MAX_DISTANCE_Y = 120f;
    const float CHECK_BOUNDARY_X = 40f;
    const float CHECK_BOUNDARY_Y = 140f;
    const float CHECK_INTERVAL = 1f;

    static readonly Vector3 _rewardSpawnPosition = new(float.MaxValue / 5, float.MaxValue / 5, 0f);

    int _currentPresetIndex = 0;

    RewardControllerConfig _config;
    Reward[] _rewards;

    private Vector2 DistanceCheckRewardPosition { get; set; }

    public async void Initialize(RewardControllerConfig config, Transform rewardParentTransform)
    {
        _config = config;
        var rewardCount = _config.GetMaxRewardCount();

        await CreateRewards(rewardCount, rewardParentTransform);

        StartRewardCheckLoop().Forget();
    }

    private async UniTask CreateRewards(int count, Transform rewardParentTransform)
    {
        var rewardPrefab = await LoadPrefabs(_config.RewardPrefabReference);

        if (rewardPrefab != null)
        {
            var diContainer = GameplaySceneInstaller.DiContainer;
            _rewards = new Reward[count];

            for (int i = 0; i < count; i++)
            {
                var reward = diContainer.InstantiatePrefabForComponent<Reward>(rewardPrefab, _rewardSpawnPosition, Quaternion.identity, rewardParentTransform);

                reward.gameObject.SetActive(false);

                _rewards[i] = reward;
            }
        }
        else
        {
            Debug.LogError("Reward prefab == null");
        }

        await UniTask.CompletedTask;
    }

    private async UniTask<GameObject> LoadPrefabs(AssetReference assetReference)
    {
        var loadOperationData = await AddressableLouderHelper.LoadAssetAsync<GameObject>(assetReference);

        return loadOperationData.LoadAsset;
    }

    private async UniTaskVoid StartRewardCheckLoop()
    {
        while (true)
        {
            CheckAndRelocateRewards();
            await UniTask.WaitForSeconds(CHECK_INTERVAL);
        }
    }

    private void CheckAndRelocateRewards()
    {
        var playerPosition = new Vector2(CharacterPositionMeter.XPosition, CharacterPositionMeter.YPosition);
        var rewardPresets = _config.GetRewardPresets();

        var centralRewardPosition = DistanceCheckRewardPosition;

        var currentPreset = rewardPresets[_currentPresetIndex];
        var presetSize = currentPreset.Size;

        var distanceX = Mathf.Abs(playerPosition.x - centralRewardPosition.x) - presetSize.x / 2;
        var distanceY = Mathf.Abs(playerPosition.y - centralRewardPosition.y) - presetSize.y / 2;

        if (distanceX > CHECK_BOUNDARY_X || distanceY > CHECK_BOUNDARY_Y)
        {
            var rewardPositions = currentPreset.RewardPositions;
            var presetCenterPosition = CalculatePresetCenterPosition(playerPosition);
            DistanceCheckRewardPosition = presetCenterPosition;

            for (int i = 0; i < rewardPositions.Length; i++)
            {
                var newRewardPosition = CalculateRewardPosition(presetCenterPosition, rewardPositions[i]);
                _rewards[i].SetNewPosition(newRewardPosition);
            }
            _currentPresetIndex = (_currentPresetIndex + 1) % rewardPresets.Length;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector2 CalculatePresetCenterPosition(Vector2 playerPosition)
    {
        var randomXDistance = Random.Range(MIN_DISTANCE_X, MAX_DISTANCE_X);

        var randomYDistance = Random.Range(MIN_DISTANCE_Y, MAX_DISTANCE_Y);

        var newX = playerPosition.x + randomXDistance;
        var newY = playerPosition.y - randomYDistance;

        return new Vector2(newX, newY);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector2 CalculateRewardPosition(Vector2 presetCenterPosition, Vector2 rewardPosition)
    {
        return presetCenterPosition + rewardPosition;
    }

}
