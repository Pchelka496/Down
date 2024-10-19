using UnityEngine;

[CreateAssetMenu(fileName = "PlayerModuleLoaderConfig", menuName = "Scriptable Objects/PlayerModuleLoaderConfig")]
public class PlayerModuleLoaderConfig : ScriptableObject
{
    [SerializeField] string _flightModuleAddressPrefab;
    [SerializeField] string _groundMovementModuleAddressPrefab;
    [SerializeField] string[] _supportModules;

    public string FlightModuleAddressPrefab { get => _flightModuleAddressPrefab; set => _flightModuleAddressPrefab = value; }
    public string GroundMovementModuleAddressPrefab { get => _groundMovementModuleAddressPrefab; set => _groundMovementModuleAddressPrefab = value; }
    public string[] SupportModules { get => _supportModules; set => _supportModules = value; }

}
