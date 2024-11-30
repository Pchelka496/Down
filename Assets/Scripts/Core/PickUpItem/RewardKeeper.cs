public class RewardKeeper
{
    readonly RewardKeeperConfig _config;

    public RewardKeeper(RewardKeeperConfig config)
    {
        _config = config;
    }

    public int GetPoints() => _config.GetPoints();

    public bool TryDecreasePoints(int decreaseValue)
    {
        if (GetPoints() >= decreaseValue)
        {
            DecreasePoints(decreaseValue);

            return true;
        }
        else
        {
            return false;
        }
    }

    public void IncreasePoints(int increaseValue) => _config.IncreasePoints(increaseValue);

    public void DecreasePoints(int decreaseValue) => _config.DecreasePoints(decreaseValue);

}
