using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ScriptableObject.Map
{
    [CreateAssetMenu(fileName = "MapControllerConfig", menuName = "Scriptable Objects/MapControllerConfig")]
    public class MapControllerConfig : UnityEngine.ScriptableObject
    {
        [SerializeField] AssetReference _prefabCheckpointPlatformAddress;
        [SerializeField] AssetReference _prefabEarthSurface;

        public AssetReference PrefabCheckpointPlatformAddress { get => _prefabCheckpointPlatformAddress; set => _prefabCheckpointPlatformAddress = value; }
        public AssetReference PrefabEarthSurface { get => _prefabEarthSurface; set => _prefabEarthSurface = value; }

    }
}
