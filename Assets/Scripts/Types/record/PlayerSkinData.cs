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

    public AssetReference SkinObjectPrefab { get => _skinObjectPrefab; }
    public AssetReference UISkinIcon { get => _skinSprite; }
    public bool IsUnlocked { get => _isUnlocked; }
    public EnumUnlockMethod UnlockMethod { get => _unlockMethod; }
    public int Cost { get => _cost; }

    public Func<bool> UnlockMethods()
    {
        switch (_unlockMethod)
        {
            case EnumUnlockMethod.Free:
                {
                    return () =>
                    {
                        _isUnlocked = true;
                        return true;
                    };
                }
            case EnumUnlockMethod.BuyForMoney:
                {
                    return () =>
                    {
                        var rewardKeeper = GameplaySceneInstaller.DiContainer.Resolve<RewardKeeper>();

                        var unlockStatus = rewardKeeper.TryDecreasePoints(_cost);
                        _isUnlocked = unlockStatus;

                        return unlockStatus;
                    };
                }
            default:
                {
                    return () => false;
                }
        }
    }

    public enum EnumUnlockMethod
    {
        Free,
        BuyForMoney
    }

}
