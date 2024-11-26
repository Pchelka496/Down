using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "CustomizerConfig", menuName = "Scriptable Objects/CustomizerConfig")]
public class CustomizerConfig : ScriptableObject, IHaveControllerGradient
{
    [SerializeField] AssetReference _currentPlayerSprite;
    [SerializeField] Gradient _currentControllerGradient;
    [SerializeField] AssetReference[] _playerSprites;

    public AssetReference PlayerSprite { get => _currentPlayerSprite; set => _currentPlayerSprite = value; }
    public Gradient ControllerGradient { get => _currentControllerGradient; set => _currentControllerGradient = value; }
    public AssetReference[] AllPlayerSprites
    {
        get
        {
            var copy = new AssetReference[_playerSprites.Length];
            System.Array.Copy(_playerSprites, copy, _playerSprites.Length);

            return copy;
        }
    }

}
