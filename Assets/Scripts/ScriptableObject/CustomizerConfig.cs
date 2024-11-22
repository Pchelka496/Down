using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "CustomizerConfig", menuName = "Scriptable Objects/CustomizerConfig")]
public class CustomizerConfig : ScriptableObject
{
    [SerializeField] AssetReference _playerSprite;
    [SerializeField] Gradient _controllerGradient;

    public AssetReference PlayerSprite { get => _playerSprite; set => _playerSprite = value; }
    public Gradient ControllerGradient { get => _controllerGradient; set => _controllerGradient = value; }

}
