using ScriptableObject.PickUpItem;
using System;

public class PlayerResourcedKeeper : IDisposable
{
    readonly PlayerResourcedKeeperConfig _config;
    readonly PlayerResourcedIndicator _indicator;

    event Action<ResourceDecreaseFailedEventArgs> OnResourceDecreaseFailed;

    public PlayerResourcedKeeper(PlayerResourcedKeeperConfig config, PlayerResourcedIndicator indicator)
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

    public bool TryDecreaseMoney(int decreaseValue, bool applyDecrease = true)
        => TryDecrease(ResourceType.Money, GetMoney(), decreaseValue, DecreaseMoney, applyDecrease);

    public bool TryDecreaseDiamonds(int decreaseValue, bool applyDecrease = true) 
        => TryDecrease(ResourceType.Diamonds, GetDiamond(), decreaseValue, DecreaseDiamond, applyDecrease);

    public bool TryDecreaseEnergy(int decreaseValue, bool applyDecrease = true) 
        => TryDecrease(ResourceType.Energy, GetEnergy(), decreaseValue, DecreaseEnergy, applyDecrease);

    private bool TryDecrease(ResourceType resourceType, int currentValue, int decreaseValue, Action<int> decreaseAction, bool applyDecrease = true)
    {
        if (currentValue >= decreaseValue)
        {
            if (applyDecrease)
            {
                decreaseAction.Invoke(decreaseValue);
            }
            return true;
        }
        else
        {
            OnResourceDecreaseFailed?.Invoke(new ResourceDecreaseFailedEventArgs(resourceType, currentValue, decreaseValue));
            return false;
        }
    }

    public void IncreaseMoney(int value) => _config.IncreaseMoney(value);
    private void DecreaseMoney(int value) => _config.DecreaseMoney(value);

    public void IncreaseDiamonds(int value) => _config.IncreaseDiamond(value);
    private void DecreaseDiamond(int value) => _config.DecreaseDiamond(value);

    public void IncreaseEnergy(int value) => _config.IncreaseEnergy(value);
    private void DecreaseEnergy(int value) => _config.DecreaseEnergy(value);

    public void SubscribeToOnResourceDecreaseFailed(Action<ResourceDecreaseFailedEventArgs> callback) => OnResourceDecreaseFailed += callback;
    public void UnsubscribeFromOnResourceDecreaseFailed(Action<ResourceDecreaseFailedEventArgs> callback) => OnResourceDecreaseFailed -= callback;

    public void Dispose()
    {
        _config.UnsubscribeToOnMoneyChangedEvent(_indicator.UpdateMoneyText);
        _config.UnsubscribeToOnDiamondChangedEvent(_indicator.UpdateDiamondText);
        _config.UnsubscribeToOnEnergyChangedEvent(_indicator.UpdateEnergyText);
    }

    public enum ResourceType
    {
        Money,
        Diamonds,
        Energy
    }

    public readonly struct ResourceDecreaseFailedEventArgs
    {
        public ResourceType ResourceType { get; }
        public int CurrentAmount { get; }
        public int RequiredAmount { get; }

        public ResourceDecreaseFailedEventArgs(ResourceType resourceType, int currentAmount, int requiredAmount)
        {
            ResourceType = resourceType;
            CurrentAmount = currentAmount;
            RequiredAmount = requiredAmount;
        }
    }
}
