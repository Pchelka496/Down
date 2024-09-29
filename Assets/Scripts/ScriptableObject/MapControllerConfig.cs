using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "MapControllerConfig", menuName = "Scriptable Objects/MapControllerConfig")]
public class MapControllerConfig : ScriptableObject
{
    [SerializeField] string _prefabCurrentCheckpointPlatformAddress;
    [SerializeField] string _prefabTargetCheckpointPlatformAddress;

    [SerializeField] string _prefabWorldBorderAddress;
    [SerializeField] Vector2 _worldBorderSize;

    [Header("Levels in order, top to bottom")]
    [SerializeField] Level[] _levels;

    public float GetFirstPlatformHeight()
    {
        return _levels[0].CurrentPlatformHeight;
    }

    public Level GetLevel(float height)
    {
        if (_levels == null || _levels.Length == 0)
            return null;

        var closestLevel = _levels
            .OrderBy(level => Mathf.Abs(level.CurrentPlatformHeight - height))
            .FirstOrDefault();

        return closestLevel;
    }

    public string PrefabCurrentCheckpointPlatformAddress { get => _prefabCurrentCheckpointPlatformAddress; set => _prefabCurrentCheckpointPlatformAddress = value; }
    public string PrefabTargetCheckpointPlatformAddress { get => _prefabTargetCheckpointPlatformAddress; set => _prefabTargetCheckpointPlatformAddress = value; }
    public string PrefabWorldBorderAddress { get => _prefabWorldBorderAddress; set => _prefabWorldBorderAddress = value; }
    public Vector2 WorldBorderSize { get => _worldBorderSize; set => _worldBorderSize = value; }

    public Level[] Levels { get => _levels; set => _levels = value; }

    [System.Serializable]
    public record Level
    {
        [SerializeField] string _mapPrefabAddress;
        [SerializeField] Vector2 _mapGlobalPosition;
        [SerializeField] float _levelHeight;
        [SerializeField] float _levelWidth;
        [SerializeField] float _heightAfterTheCurrentPlatform;
        [SerializeField] float _heightHeightBeforeTheCurrentPlatform;
        [SerializeField] float _currentPlatformHeight;
        [SerializeField] float _targetPlatformHeight;
        [SerializeField] float _currentPlatformWidth;
        [SerializeField] float _targetPlatformWidth;
        [Tooltip("Checkpoint LocalPositions")]
        [SerializeField] float[] _currentPlatformDoorsXPositions;
        [Tooltip("Checkpoint LocalPositions")]
        [SerializeField] float[] _targetPlatformDoorsXPositions;

        public string MapPrefabAddress { get => _mapPrefabAddress; }
        public Vector2 MapGlobalPosition { get => _mapGlobalPosition; set => _mapGlobalPosition = value; }
        public float LevelHeight { get => _levelHeight; }
        public float LevelWidth { get => _levelWidth; }
        public float HeightBeforeTheCurrentPlatform { get => _heightHeightBeforeTheCurrentPlatform; }
        public float HeightAfterTheCurrentPlatform { get => _heightAfterTheCurrentPlatform; }
        public float CurrentPlatformHeight { get => _currentPlatformHeight; set => _currentPlatformHeight = value; }
        public float TargetPlatformHeight { get => _targetPlatformHeight; set => _targetPlatformHeight = value; }
        public float CurrentPlatformWidth { get => _currentPlatformWidth; }
        public float TargetPlatformWidth { get => _targetPlatformWidth; }
        public float[] TargetPlatformDoorsPositions
        {
            get => _targetPlatformDoorsXPositions != null ? (float[])_targetPlatformDoorsXPositions.Clone() : null;
            set => _targetPlatformDoorsXPositions = value;
        }

        public float[] CurrentPlatformDoorsPositions
        {
            get => _currentPlatformDoorsXPositions != null ? (float[])_currentPlatformDoorsXPositions.Clone() : null;
            set => _currentPlatformDoorsXPositions = value;
        }

    }

}
