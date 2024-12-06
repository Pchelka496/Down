using System;
using System.Collections.Generic;
using Types.record;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Linq;

namespace ScriptableObject
{
    [CreateAssetMenu(fileName = "CustomizerConfig", menuName = "Scriptable Objects/CustomizerConfig")]
    public class CustomizerConfig : UnityEngine.ScriptableObject,
        IHaveControllerGradient,
        IHaveSkin,
        IHaveDataForSave,
        IHaveAllPlayerSkins
    {
        [SerializeField] AssetReference _currentPlayerSprite;
        [SerializeField] Gradient _currentControllerGradient;
        [SerializeField] PlayerSkinData[] _playerSkinData;
        Action<IHaveDataForSave> _saveAction;
        event Action OnSkinChanged;
        event Action OnControllerGradientChanged;

        public AssetReference Skin
        {
            get => _currentPlayerSprite != null ? new AssetReference(_currentPlayerSprite.AssetGUID) : null;
            set
            {
                if (_currentPlayerSprite.AssetGUID == value.AssetGUID) return;

                _currentPlayerSprite = value;
                OnSkinChanged?.Invoke();
                _saveAction?.Invoke(this);
            }
        }

        public Gradient ControllerGradient
        {
            get => new()
            {
                colorKeys = _currentControllerGradient.colorKeys,
                alphaKeys = _currentControllerGradient.alphaKeys,
                mode = _currentControllerGradient.mode
            };
            set
            {
                var newGradient = new Gradient()
                {
                    colorKeys = value.colorKeys,
                    alphaKeys = value.alphaKeys,
                    mode = value.mode
                };

                if (_currentControllerGradient.AreEqual(newGradient))
                    return;

                _currentControllerGradient = newGradient;
                OnControllerGradientChanged?.Invoke();
                _saveAction?.Invoke(this);
            }
        }

        public PlayerSkinData[] AllPlayerSkins
        {
            get
            {
                var copy = new PlayerSkinData[_playerSkinData.Length];
                Array.Copy(_playerSkinData, copy, _playerSkinData.Length);

                foreach (var skin in copy)
                {
                    skin.SetSkinOpenStatusChangedAction(ChangeSkinOpenStatus);
                }

                return copy;
            }
        }

        void IHaveDataForSave.SaveToSaveData(SaveData saveData)
        {
            saveData.SkinOpenStatus = GetSkinOpenStatus();
        }

        void IHaveDataForSave.LoadSaveData(SaveData saveData)
        {
            var skinOpenStatus = saveData.SkinOpenStatus;

            LoadSkinOpenStatusFromDictionary(skinOpenStatus);
        }

        Action IHaveDataForSave.SubscribeWithUnsubscribe(Action<IHaveDataForSave> saveAction)
        {
            _saveAction = saveAction;
            return () => _saveAction = null;
        }

        private void ChangeSkinOpenStatus(string skinId, bool openStatus)
        {
            foreach (var skin in from skin in _playerSkinData
                                 where skin.SkinId == skinId
                                 select skin)
            {
                skin.IsUnlocked = openStatus;
                _saveAction?.Invoke(this);
                break;
            }
        }

        public Dictionary<string, bool> GetSkinOpenStatus()
        {
            var skinOpenStatus = new Dictionary<string, bool>();

            foreach (var skin in _playerSkinData)
            {
                skinOpenStatus[skin.SkinId] = skin.IsUnlocked;
            }

            return skinOpenStatus;
        }

        public void LoadSkinOpenStatusFromDictionary(Dictionary<string, bool> skinOpenStatus)
        {
            foreach (var skin in _playerSkinData)
            {
                if (skinOpenStatus.ContainsKey(skin.SkinId))
                {
                    skin.IsUnlocked = skinOpenStatus[skin.SkinId];
                }
                else
                {
                    Debug.LogWarning($"Skin with ID {skin.SkinId} not found in the provided dictionary.");
                }
            }
        }

        public void SubscribeToOnSkinChangedEvent(Action action) => OnSkinChanged += action;
        public void UnsubscribeToOnSkinChangedEvent(Action action) => OnSkinChanged -= action;

        public void SubscribeToOnControllerGradientChangedEvent(Action action) => OnControllerGradientChanged += action;
        public void UnsubscribeToControllerGradientChangedEvent(Action action) => OnControllerGradientChanged -= action;
    }
}