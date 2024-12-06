using System;
using Core.Installers;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Types.record
{
    [Serializable]
    public record PlayerSkinData
    {
        [SerializeField] AssetReference _skinObjectPrefab;
        [SerializeField] AssetReference _skinSprite;
        [SerializeField] bool _isUnlocked;
        [SerializeField] EnumUnlockMethod _unlockMethod;
        [SerializeField] int _cost;
        [SerializeField] string _skinId;
        Action<string, bool> _skinOpenStatusChanged;

        public AssetReference SkinObjectPrefab => _skinObjectPrefab;
        public AssetReference UISkinIcon => _skinSprite;

        public bool IsUnlocked
        {
            get => _isUnlocked;
            set
            {
                if (_isUnlocked == value) return;

                _isUnlocked = value;

                _skinOpenStatusChanged?.Invoke(_skinId, _isUnlocked);
            }
        }
        public EnumUnlockMethod UnlockMethod => _unlockMethod;
        public int Cost => _cost;
        public string SkinId => _skinId;

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

                        var unlockStatus = rewardKeeper.TryDecreaseMoney(_cost);
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

        public void SetSkinOpenStatusChangedAction(Action<string, bool> action) => _skinOpenStatusChanged = action;

        public enum EnumUnlockMethod
        {
            Free,
            BuyForMoney
        }

    }
}
