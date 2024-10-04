using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "MapControllerConfig", menuName = "Scriptable Objects/MapControllerConfig")]
public class MapControllerConfig : ScriptableObject
{
    [SerializeField] string _prefabCheckpointPlatformAddress;
    [SerializeField] string _prefabEarthSurface;
    [SerializeField] PlatformInformation[] _platforms;
    [SerializeField] float _maximumHeight;

    public string PrefabCheckpointPlatformAddress { get => _prefabCheckpointPlatformAddress; set => _prefabCheckpointPlatformAddress = value; }
    public string PrefabEarthSurface { get => _prefabEarthSurface; set => _prefabEarthSurface = value; }
    public float MaximumHeight { get => _maximumHeight; }

    public float FirstPlatformHeight()
    {
        if (_platforms == null || _platforms.Length == 0)
            return default;

        return _platforms[0].PlatformHeight;
    }

    public float HighestSavingHeight()
    {
        if (_platforms == null || _platforms.Length == 0)
            return default;

        return _platforms[_platforms.Length - 1].PlatformHeight;
    }

    public float GetSavingHeight(float height)
    {
        if (_platforms == null || _platforms.Length == 0)
            return default;

        var closestHeight = _platforms
            .OrderBy(platform => Mathf.Abs(platform.PlatformHeight - height))
            .FirstOrDefault();

        return closestHeight.PlatformHeight;
    }

    [System.Serializable]
    public record PlatformInformation
    {
        [SerializeField] int _pointsForOpen;
        [SerializeField] bool _isOpen;
        [SerializeField] float _platformHeight;

        public int PointsForOpen { get => _pointsForOpen; set => _pointsForOpen = value; }
        public bool IsOpen { get => _isOpen; set => _isOpen = value; }
        public float PlatformHeight { get => _platformHeight; set => _platformHeight = value; }
    }

#if UNITY_EDITOR
    public PlatformInformation[] Platforms { get => _platforms; set => _platforms = value; }

#endif

}
