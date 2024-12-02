using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "CustomizerConfig", menuName = "Scriptable Objects/CustomizerConfig")]
public class CustomizerConfig : ScriptableObject, IHaveControllerGradient, IHaveSkin, IHaveAllPlayerSkins
{
    [SerializeField] AssetReference _currentPlayerSprite;
    [SerializeField] Gradient _currentControllerGradient;
    [SerializeField] PlayerSkinData[] _playerSkinData;
    event Action OnSkinChanged;
    event Action OnControllerGradientChanged;
    event Action<Dictionary<string, bool>> OnSkinOpenStatusChanged;

    public AssetReference Skin
    {
        get => _currentPlayerSprite != null ? new AssetReference(_currentPlayerSprite.AssetGUID) : null;
        set
        {
            if (_currentPlayerSprite.AssetGUID == value.AssetGUID) return;

            _currentPlayerSprite = value;
            OnSkinChanged?.Invoke();
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
        }
    }

    public PlayerSkinData[] AllPlayerSkins
    {
        get
        {
            var copy = new PlayerSkinData[_playerSkinData.Length];
            System.Array.Copy(_playerSkinData, copy, _playerSkinData.Length);

            foreach (var skin in copy)
            {
                skin.SetSkinOpenStatusChangedAction(ChangeSkinOpenStatus);
            }

            return copy;
        }
    }

    public void LoadSaveData(SaveData saveData)
    {
        var skinOpenStatus = saveData.SkinOpenStatus;
        LoadSkinOpenStatusFromDictionary(skinOpenStatus);
        UpdateAndSendSkinOpenStatus();
    }

    private void ChangeSkinOpenStatus(string skinId, bool openStatus)
    {
        foreach (var skin in _playerSkinData)
        {
            if (skin.SkinId == skinId)
            {
                skin.IsUnlocked = openStatus;
                break;
            }
        }
        UpdateAndSendSkinOpenStatus();
    }

    public void UpdateAndSendSkinOpenStatus()
    {
        var skinOpenStatus = new Dictionary<string, bool>();

        foreach (var skin in _playerSkinData)
        {
            skinOpenStatus[skin.SkinId] = skin.IsUnlocked;
        }

        OnSkinOpenStatusChanged?.Invoke(skinOpenStatus);
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

    public void SubscribeToOnSkinOpenStatusChanged(Action<Dictionary<string, bool>> action) => OnSkinOpenStatusChanged += action;
    public void UnsubscribeToOnSkinOpenStatusChanged(Action<Dictionary<string, bool>> action) => OnSkinOpenStatusChanged -= action;

}

