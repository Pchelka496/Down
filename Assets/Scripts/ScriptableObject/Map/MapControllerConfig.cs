using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "MapControllerConfig", menuName = "Scriptable Objects/MapControllerConfig")]
public class MapControllerConfig : ScriptableObject
{
    [SerializeField] string _prefabCheckpointPlatformAddress;
    [SerializeField] string _prefabEarthSurface;

    public string PrefabCheckpointPlatformAddress { get => _prefabCheckpointPlatformAddress; set => _prefabCheckpointPlatformAddress = value; }
    public string PrefabEarthSurface { get => _prefabEarthSurface; set => _prefabEarthSurface = value; }

}
