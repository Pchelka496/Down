using UnityEngine;

[CreateAssetMenu(fileName = "MapControllerConfig", menuName = "Scriptable Objects/MapControllerConfig")]
public class MapControllerConfig : ScriptableObject
{
    [SerializeField] string _prefabCurrentCheckpointPlatformAddress;
    [SerializeField] string _prefabTargetCheckpointPlatformAddress;

    [SerializeField] string _prefabWorldBorderAddress;
    [SerializeField] Vector2 _worldBorderSize;

    [Header("Levels in order, top to bottom")]
    [SerializeField] Level[] _levels;

    public string PrefabCurrentCheckpointPlatformAddress { get => _prefabCurrentCheckpointPlatformAddress; set => _prefabCurrentCheckpointPlatformAddress = value; }
    public string PrefabTargetCheckpointPlatformAddress { get => _prefabTargetCheckpointPlatformAddress; set => _prefabTargetCheckpointPlatformAddress = value; }
    public string PrefabWorldBorderAddress { get => _prefabWorldBorderAddress; set => _prefabWorldBorderAddress = value; }
    public Vector2 WorldBorderSize { get => _worldBorderSize; set => _worldBorderSize = value; }

    public Level[] Levels { get => _levels; set => _levels = value; }

    [System.Serializable]
    public record Level
    {
        [SerializeField] string _mapPrefabAddress;
        [SerializeField] float _levelHeight;
        [SerializeField] float _levelWidth;
        [SerializeField] float _heightAfterTheCurrentPlatform;
        [SerializeField] float _heightHeightBeforeTheCurrentPlatform;
        [SerializeField] float _currentPlatformHeight;
        [SerializeField] float _targetPlatformHeight;

        public string MapPrefabAddress { get => _mapPrefabAddress; set => _mapPrefabAddress = value; }
        public float LevelHeight { get => _levelHeight; set => _levelHeight = value; }
        public float LevelWidth { get => _levelWidth; set => _levelWidth = value; }
        public float HeightHeightBeforeTheCurrentPlatform { get => _heightHeightBeforeTheCurrentPlatform; set => _heightHeightBeforeTheCurrentPlatform = value; }
        public float HeightAfterTheCurrentPlatform { get => _heightAfterTheCurrentPlatform; set => _heightAfterTheCurrentPlatform = value; }
        public float CurrentPlatformHeight { get => _currentPlatformHeight; set => _currentPlatformHeight = value; }
        public float TargetPlatformHeight { get => _targetPlatformHeight; set => _targetPlatformHeight = value; }
    }

}
