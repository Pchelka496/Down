using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public record PlayerSkinData
{
    [SerializeField] AssetReference _skinObjectPrefab;
    [SerializeField] AssetReference _skinSprite;
    [SerializeField] bool _isUnlocked;
    [SerializeField] EnumUnlockMethod _unlockMethod;
    [SerializeField] int _cost;
    [SerializeField] string _skinId;
    Action<string, bool> SkinOpenStatusChanged;

    public AssetReference SkinObjectPrefab { get => _skinObjectPrefab; }
    public AssetReference UISkinIcon { get => _skinSprite; }
    public bool IsUnlocked
    {
        get
        {
            return _isUnlocked;
        }
        set
        {
            if (_isUnlocked == value) return;

            _isUnlocked = value;

            SkinOpenStatusChanged?.Invoke(_skinId, _isUnlocked);
        }
    }
    public EnumUnlockMethod UnlockMethod { get => _unlockMethod; }
    public int Cost { get => _cost; }
    public string SkinId { get => _skinId; }

    public Func<bool> UnlockMethods()
    {
        switch (_unlockMethod)
        {
            case EnumUnlockMethod.Free:
                {
                    return () =>
                    {
                        IsUnlocked = true;
                        return true;
                    };
                }
            case EnumUnlockMethod.BuyForMoney:
                {
                    return () =>
                    {
                        var rewardKeeper = GameplaySceneInstaller.DiContainer.Resolve<RewardKeeper>();

                        var unlockStatus = rewardKeeper.TryDecreasePoints(_cost);
                        IsUnlocked = unlockStatus;

                        return unlockStatus;
                    };
                }
            default:
                {
                    return () => false;
                }
        }
    }

    public void SetSkinOpenStatusChangedAction(Action<string, bool> action) => SkinOpenStatusChanged = action;

    public enum EnumUnlockMethod
    {
        Free,
        BuyForMoney
    }

}
