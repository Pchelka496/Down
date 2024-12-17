using System;
using Types.record;

public interface IHaveDataForSave
{
    public void SaveToSaveData(SaveData saveData);
    public void LoadSaveData(SaveData saveData);

    public Action SubscribeWithUnsubscribe(Action<IHaveDataForSave> saveAction);
}
