using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "MapControllerConfig", menuName = "Scriptable Objects/MapControllerConfig")]
public class MapControllerConfig : ScriptableObject
{
    [SerializeField] string _prefabCheckpointPlatformAddress;
    [SerializeField] string _prefabEarthSurface;
    [SerializeField] float[] _savingHeights;

    public string PrefabCheckpointPlatformAddress { get => _prefabCheckpointPlatformAddress; set => _prefabCheckpointPlatformAddress = value; }
    public string PrefabEarthSurface { get => _prefabEarthSurface; set => _prefabEarthSurface = value; }

    public float FirstPlatformHeight()
    {
        if (_savingHeights == null || _savingHeights.Length == 0)
            return default;

        return _savingHeights[0];
    }

    public float HighestSavingHeight()
    {
        if (_savingHeights == null || _savingHeights.Length == 0)
            return default;

        return _savingHeights[_savingHeights.Length - 1];
    }

    public float GetSavingHeight(float height)
    {
        if (_savingHeights == null || _savingHeights.Length == 0)
            return default;

        var closestHeight = _savingHeights
            .OrderBy(level => Mathf.Abs(level - height))
            .FirstOrDefault();

        return closestHeight;
    }

#if UNITY_EDITOR
    public float[] SavingHeights { get => _savingHeights; set => _savingHeights = value; }

#endif

}
