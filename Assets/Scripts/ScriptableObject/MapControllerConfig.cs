using UnityEngine;

[CreateAssetMenu(fileName = "MapControllerConfig", menuName = "Scriptable Objects/MapControllerConfig")]
public class MapControllerConfig : ScriptableObject
{
    [SerializeField] string _mapPrefabAddress;

}
