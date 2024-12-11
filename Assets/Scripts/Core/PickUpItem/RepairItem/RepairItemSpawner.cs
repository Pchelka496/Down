using Additional;
using Core;
using Core.Installers;
using Cysharp.Threading.Tasks;
using ScriptableObject.PickUpItem;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RepairItemSpawner : System.IDisposable
{
    const float REPAIR_ITEM_X_SPAWN_POSITION = LevelManager.PLAYER_START_X_POSITION;
    const float REPAIR_ITEM_Y_SPAWN_POSITION = LevelManager.PLAYER_START_Y_POSITION + 1000f;

    const float X_MIN_BOUND = -50f;
    const float X_MAX_BOUND = 50f;
    const float Y_MIN_BOUND = -50f;
    const float Y_MAX_BOUND = -200f;
    const float MIN_PLAYER_DISTANCE = 220f;

    const float CHECK_INTERVAL = 1f;

    float _lowestRepairItemHeight = REPAIR_ITEM_Y_SPAWN_POSITION;
    RepairKitControllerConfig _config;
    RepairItem[] _repairItem;

    CancellationTokenSource _relocateCheckLoopCts;

    event System.Action DisposeEvents;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(GlobalEventsManager globalEventsManager)
    {
        globalEventsManager.SubscribeToRoundStarted(RoundStart);
        globalEventsManager.SubscribeToRoundEnded(RoundEnd);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
    }

    public async void Initialize(RepairKitControllerConfig config, Transform repairKitParentTransform)
    {
        _config = config;
        var repairKitCount = _config.MaxRepairKitCount;

        await CreateNewRepairItem(repairKitCount, repairKitParentTransform);
    }

    private void RoundStart()
    {
        ClearToken();
        _relocateCheckLoopCts = new();

        StartRepairItemCheckLoop(_relocateCheckLoopCts.Token).Forget();
    }

    private void RoundEnd()
    {
        ClearToken();
        RelocateAllRewards();
    }

    private void RelocateAllRewards()
    {
        foreach (var repairItem in _repairItem)
        {
            repairItem.Relocate(new(REPAIR_ITEM_X_SPAWN_POSITION, REPAIR_ITEM_Y_SPAWN_POSITION));
        }
    }

    private async UniTask CreateNewRepairItem(int count, Transform repairKitParentTransform)
    {
        var rewardPrefab = await LoadPrefabs(_config.RewardPrefabAddress);

        if (rewardPrefab != null)
        {
            var diContainer = GameplaySceneInstaller.DiContainer;
            _repairItem = new RepairItem[count];

            for (int i = 0; i < count; i++)
            {
                var repairKit = diContainer.InstantiatePrefabForComponent<RepairItem>(rewardPrefab, new(REPAIR_ITEM_X_SPAWN_POSITION, REPAIR_ITEM_Y_SPAWN_POSITION), Quaternion.identity, repairKitParentTransform);

                repairKit.gameObject.SetActive(false);

                _repairItem[i] = repairKit;
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

    private async UniTaskVoid StartRepairItemCheckLoop(CancellationToken token)
    {
        var timeSpanDelay = System.TimeSpan.FromSeconds(CHECK_INTERVAL);

        while (true)
        {
            if (Mathf.Abs(CharacterPositionMeter.YPosition - _lowestRepairItemHeight) > MIN_PLAYER_DISTANCE)
            {
                RelocateRepairItem();
            }

            await UniTask.Delay(timeSpanDelay, cancellationToken: token);
        }
    }

    private void RelocateRepairItem()
    {
        var playerXPosition = CharacterPositionMeter.XPosition;
        var playerYPosition = CharacterPositionMeter.YPosition;

        _lowestRepairItemHeight = float.MaxValue;

        for (int i = 0; i < _repairItem.Length; i++)
        {
            var xPosition = playerXPosition + Mathf.Lerp(X_MIN_BOUND, X_MAX_BOUND, (float)i / (_repairItem.Length - 1));
            var yPosition = playerYPosition + Random.Range(Y_MIN_BOUND, Y_MAX_BOUND);

            _repairItem[i].Relocate(new Vector2(xPosition, yPosition));

            if (yPosition < _lowestRepairItemHeight)
            {
                _lowestRepairItemHeight = yPosition;
            }
        }
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _relocateCheckLoopCts);

    public void Dispose()
    {
        ClearToken();
        DisposeEvents?.Invoke();
    }
}
