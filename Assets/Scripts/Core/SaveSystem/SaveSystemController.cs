using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Types.record;

namespace Core.SaveSystem
{
    public class SaveSystemController
    {
        const float SAVE_DELAY = 1.0f;
        readonly JsonSaveSystem _saveSystem = new();
        readonly object _lock = new();

        readonly SaveData _saveData;
        volatile bool _needToSave;
        readonly List<Action> _unsubscribeToChangeDataEventActions = new();

        public SaveSystemController(IHaveDataForSave[] haveDataForSaveArray)
        {
            _saveData = _saveSystem.Load();

            HaveDataForSaveProcessing(haveDataForSaveArray, _saveData);
            FullSaving(haveDataForSaveArray);

            UniTask.RunOnThreadPool(BackgroundSaveTask).Forget();
        }

        private void HaveDataForSaveProcessing(IHaveDataForSave[] haveDataForSaveArray, SaveData saveData)
        {
            foreach (var haveData in haveDataForSaveArray)
            {
                haveData.LoadSaveData(saveData);

                var unsubscribeAction = haveData.SubscribeWithUnsubscribe(SaveForSaveData);
                _unsubscribeToChangeDataEventActions.Add(unsubscribeAction);
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void FullSaving(IHaveDataForSave[] haveDataForSaveArray)
        {
            lock (_lock)
            {
                foreach (var haveDataForSave in haveDataForSaveArray)
                {
                    haveDataForSave.SaveToSaveData(_saveData);
                }
            }

            Save();
        }

        private void SaveForSaveData(IHaveDataForSave needDataSaveObject)
        {
            lock (_lock)
            {
                needDataSaveObject.SaveToSaveData(_saveData);
                _needToSave = true;
            }
        }

        private async UniTaskVoid BackgroundSaveTask()
        {
            while (true)
            {
                if (_needToSave)
                {
                    Save();
                }

                await UniTask.WaitForSeconds(SAVE_DELAY);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void Save()
        {
            lock (_lock)
            {
                _saveSystem.Save(_saveData);
                _needToSave = false;
            }
        }

        public void Dispose()
        {
            Save();

            lock (_lock)
            {
                foreach (var action in _unsubscribeToChangeDataEventActions)
                {
                    action?.Invoke();
                }
            }
        }
    }
}