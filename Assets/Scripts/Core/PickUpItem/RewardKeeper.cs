using ScriptableObject.PickUpItem;

public class RewardKeeper : System.IDisposable
{
    readonly RewardKeeperConfig _config;
    readonly RewardIndicator _indicator;

    public RewardKeeper(RewardKeeperConfig config, RewardIndicator indicator)
    {
        _config = config;
        _indicator = indicator;

        config.SubscribeToOnMoneyChangedEvent(indicator.UpdateMoneyText);
        config.SubscribeToOnDiamondChangedEvent(indicator.UpdateDiamondText);
        config.SubscribeToOnEnergyChangedEvent(indicator.UpdateEnergyText);
    }

    public int GetMoney() => _config.GetMoney();
    public int GetDiamond() => _config.GetDiamond();
    public int GetEnergy() => _config.GetEnergy();

    public bool TryDecreaseMoney(int decreaseValue) => TryDecrease(GetMoney(), decreaseValue, DecreaseMoney);
    public bool TryDecreaseDiamonds(int decreaseValue) => TryDecrease(GetDiamond(), decreaseValue, DecreaseDiamond);
    public bool TryDecreaseEnergy(int decreaseValue) => TryDecrease(GetEnergy(), decreaseValue, DecreaseEnergy);

    public bool TryDecrease(int currentValue, int decreaseValue, System.Action<int> decreaseAction)
    {
        if (currentValue >= decreaseValue)
        {
            decreaseAction.Invoke(decreaseValue);

            return true;
        }
        else
        {
            return false;
        }
    }

    public void IncreaseMoney(int value) => _config.IncreaseMoney(value);
    public void DecreaseMoney(int value) => _config.DecreaseMoney(value);

    public void IncreaseDiamonds(int value) => _config.IncreaseDiamond(value);
    public void DecreaseDiamond(int value) => _config.DecreaseDiamond(value);

    public void IncreaseEnergy(int value) => _config.IncreaseEnergy(value);
    public void DecreaseEnergy(int value) => _config.DecreaseEnergy(value);

    public void Dispose()
    {
        _config.UnsubscribeToOnMoneyChangedEvent(_indicator.UpdateMoneyText);
        _config.UnsubscribeToOnDiamondChangedEvent(_indicator.UpdateDiamondText);
        _config.UnsubscribeToOnEnergyChangedEvent(_indicator.UpdateEnergyText);
    }
}
