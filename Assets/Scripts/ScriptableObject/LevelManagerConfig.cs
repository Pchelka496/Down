using UnityEngine;

[CreateAssetMenu(fileName = "LevelManagerConfig", menuName = "Scriptable Objects/LevelManagerConfig")]
public class LevelManagerConfig : ScriptableObject//one per project
{
    public const float NO_SAVED_HEIGHT = float.NaN;
    [SerializeField] MapControllerConfig _mapControllerConfig;
    [SerializeField] BackgroundControllerConfig _backgroundControllerConfig;
    [SerializeField] float _playerSavedHeight = NO_SAVED_HEIGHT;

    public float PlayerSavedHeight { get => _playerSavedHeight; set => _playerSavedHeight = value; }
    public MapControllerConfig MapControllerConfig { get => _mapControllerConfig; set => _mapControllerConfig = value; }
    public BackgroundControllerConfig BackgroundControllerConfig { get => _backgroundControllerConfig; set => _backgroundControllerConfig = value; }

}
