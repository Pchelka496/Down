using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class RepairKitController
{
    const float REPAIR_KIT_X_SPAWN_POSITION = LevelManager.PLAYER_START_X_POSITION;
    const float REPAIR_KIT_Y_SPAWN_POSITION = LevelManager.PLAYER_START_Y_POSITION + 1000f;

    const float X_MIN_BOUND = -50f;
    const float X_MAX_BOUND = 50f;
    const float Y_MIN_BOUND = -50f;
    const float Y_MAX_BOUND = -200f;
    const float MIN_PLAYER_DISTANCE = 220f;

    float _lowestRepairKitHeight = REPAIR_KIT_Y_SPAWN_POSITION;
    RepairKitControllerConfig _config;
    RepairKit[] _repairKit;

    public async void Initialize(RepairKitControllerConfig config, Transform repairKitParentTransform)
    {
        _config = config;
        var repairKitCount = _config.MaxRepairKitCount;

        await CreateRewards(repairKitCount, repairKitParentTransform);

        StartRepairKitCheckLoop().Forget();
    }

    private async UniTask CreateRewards(int count, Transform repairKitParentTransform)
    {
        var rewardPrefab = await LoadPrefabs(_config.RewardPrefabAddress);

        if (rewardPrefab != null)
        {
            var diContainer = GameplaySceneInstaller.DiContainer;
            _repairKit = new RepairKit[count];

            for (int i = 0; i < count; i++)
            {
                var repairKit = diContainer.InstantiatePrefabForComponent<RepairKit>(rewardPrefab, new(REPAIR_KIT_X_SPAWN_POSITION, REPAIR_KIT_Y_SPAWN_POSITION), Quaternion.identity, repairKitParentTransform);

                repairKit.gameObject.SetActive(false);

                _repairKit[i] = repairKit;
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

    private async UniTaskVoid StartRepairKitCheckLoop()
    {
        while (true)
        {
            RelocateRepairKit();

            await UniTask.WaitUntil(() => Mathf.Abs(CharacterPositionMeter.YPosition - _lowestRepairKitHeight) > MIN_PLAYER_DISTANCE);
        }
    }

    private void RelocateRepairKit()
    {
        var playerXPosition = CharacterPositionMeter.XPosition;
        var playerYPosition = CharacterPositionMeter.YPosition;
        _lowestRepairKitHeight = float.MaxValue;

        for (int i = 0; i < _repairKit.Length; i++)
        {
            var xPosition = playerXPosition + Mathf.Lerp(X_MIN_BOUND, X_MAX_BOUND, (float)i / (_repairKit.Length - 1));
            var yPosition = playerYPosition + Random.Range(Y_MIN_BOUND, Y_MAX_BOUND);

            _repairKit[i].Relocate(new Vector2(xPosition, yPosition));

            if (yPosition < _lowestRepairKitHeight)
            {
                _lowestRepairKitHeight = yPosition;
            }
        }
    }

}
