using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

public class SaveSystemController
{
    readonly RewardKeeperConfig _rewardKeeperConfig;
    readonly CustomizerConfig _customizerConfig;
    readonly SaveSystem _saveSystem = new();
    readonly object _lock = new();

    readonly SaveData _saveData;
    volatile bool _needToSave;

    public SaveSystemController(RewardKeeperConfig rewardKeeperConfig, CustomizerConfig customizerConfig)
    {
        _saveData = _saveSystem.Load();

        _rewardKeeperConfig = rewardKeeperConfig;
        _customizerConfig = customizerConfig;

        _rewardKeeperConfig.LoadSaveData(_saveData);
        _customizerConfig.LoadSaveData(_saveData);

        _rewardKeeperConfig.SubscribeToOnPointChangedEvent(ChangePoints);
        _customizerConfig.SubscribeToOnSkinOpenStatusChanged(ChangeSkinOpenStatus);

        UniTask.RunOnThreadPool(BackgroundSaveTask, true).Forget();
    }

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundStart(RoundStart);

    }

    private void RoundStart(LevelManager levelManager) => levelManager.SubscribeToRoundEnd(RoundEnd);

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);

    }

    private void ChangePoints(int newPoints)
    {
        lock (_lock)
        {
            _saveData.Points = newPoints;
            _needToSave = true;
        }
    }

    private void ChangeSkinOpenStatus(Dictionary<string, bool> skinOpenStatus)
    {
        lock (_lock)
        {
            _saveData.SkinOpenStatus = skinOpenStatus;
            _needToSave = true;
        }
    }

    public void OnApplicationQuit()
    {
        Save();
    }

    private async UniTaskVoid BackgroundSaveTask()
    {
        while (true)
        {
            if (_needToSave)
            {
                Save();
            }

            await UniTask.WaitForSeconds(1);
        }
    }

    private void Save()
    {
        lock (_lock)
        {
            UnityEngine.Debug.Log("save");
            _saveSystem.Save(_saveData);
            _needToSave = false;
        }
    }

    public void Dispose()
    {
        Save();

        lock (_lock)
        {
            _rewardKeeperConfig.UnsubscribeToOnPointChangedEvent(ChangePoints);
            _customizerConfig.UnsubscribeToOnSkinOpenStatusChanged(ChangeSkinOpenStatus);
        }
    }

}
