using UnityEngine;

[CreateAssetMenu(fileName = "LevelManagerConfig", menuName = "Scriptable Objects/LevelManagerConfig")]
public class LevelManagerConfig : ScriptableObject//one per project
{
    [SerializeField] EnemyManagerConfig _enemyManagerConfig;
    [SerializeField] MapControllerConfig _mapControllerConfig;
    [SerializeField] BackgroundControllerConfig _backgroundControllerConfig;
    [SerializeField] RewardManagerConfig _rewardManagerConfig;

    public EnemyManagerConfig EnemyManagerConfig { get => _enemyManagerConfig; }
    public MapControllerConfig MapControllerConfig { get => _mapControllerConfig; }
    public BackgroundControllerConfig BackgroundControllerConfig { get => _backgroundControllerConfig; }
    public RewardManagerConfig RewardManagerConfig { get => _rewardManagerConfig; set => _rewardManagerConfig = value; }

}
