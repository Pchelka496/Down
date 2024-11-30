using System;
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

            return copy;
        }
    }

    public void SubscribeToOnSkinChangedEvent(Action action) => OnSkinChanged += action;
    public void UnsubscribeToOnSkinChangedEvent(Action action) => OnSkinChanged -= action;

    public void SubscribeToOnControllerGradientChangedEvent(Action action) => OnControllerGradientChanged += action;
    public void UnsubscribeToControllerGradientChangedEvent(Action action) => OnControllerGradientChanged -= action;

}
