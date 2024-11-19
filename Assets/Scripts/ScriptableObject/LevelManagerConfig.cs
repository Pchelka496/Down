using UnityEngine;

[CreateAssetMenu(fileName = "LevelManagerConfig", menuName = "Scriptable Objects/LevelManagerConfig")]
public class LevelManagerConfig : ScriptableObject//one per project
{
    [SerializeField] EnemyManagerConfig _enemyManagerConfig;
    [SerializeField] MapControllerConfig _mapControllerConfig;
    [SerializeField] BackgroundControllerConfig _backgroundControllerConfig;
    [SerializeField] PickUpItemManagerConfig _pickUpItemManagerConfig;

    public EnemyManagerConfig EnemyManagerConfig { get => _enemyManagerConfig; }
    public MapControllerConfig MapControllerConfig { get => _mapControllerConfig; }
    public BackgroundControllerConfig BackgroundControllerConfig { get => _backgroundControllerConfig; }
    public PickUpItemManagerConfig RewardManagerConfig { get => _pickUpItemManagerConfig; set => _pickUpItemManagerConfig = value; }

}
