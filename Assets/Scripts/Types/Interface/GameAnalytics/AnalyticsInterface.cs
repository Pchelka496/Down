public interface ICollectedMoneyTracker
{
    public int GetCollectedMoney();
    public void SubscribeToMoneyChanged(System.Action<int> callback);
    public void UnsubscribeFromMoneyChanged(System.Action<int> callback);
}

public interface IPlayerCollisionTracker
{
    public int GetCollisionCount();
    public void SubscribeToCollisionChanged(System.Action<int> callback);
    public void UnsubscribeFromCollisionChanged(System.Action<int> callback);
}

public interface IRoundResultTracker
{
    public EnumRoundResult GetRoundResult();
    public void SubscribeToRoundResultChanged(System.Action<EnumRoundResult> callback);
    public void UnsubscribeFromRoundResultChanged(System.Action<EnumRoundResult> callback);
}

public enum EnumRoundResult
{
    Unfinished,
    Victory,
    Defeat
}
