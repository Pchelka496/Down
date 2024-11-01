using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using static EnemyConfig;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "EnemyManagerConfig", menuName = "Scriptable Objects/EnemyManagerConfig")]
public class EnemyManagerConfig : ScriptableObject
{
    [SerializeField] AssetReference _enemyCorePrefab;
    [SerializeField] int _enemyCount;
    [SerializeField] int _challengeCount;
    [SerializeField] EnemyRegionConfig[] _enemyRegions;
    [SerializeField] EnemyConfig[] _challengePrefabs;

    public int AllEnemyCount => _enemyCount + _challengeCount;
    public int EnemyCount { get => _enemyCount; }
    public int ChallengeCount { get => _challengeCount; }

    public AssetReference EnemyCorePrefab { get => _enemyCorePrefab; set => _enemyCorePrefab = value; }
    public Stack<EnemyRegionConfig> EnemyRegions
    {
        get
        {
            return new Stack<EnemyRegionConfig>(_enemyRegions.OrderBy(region => region.StartHeight));
        }
    }

    public Enemy[] ChallengeEnemies => _challengePrefabs
                                      .Select(config => config.GetEnemy)
                                      .ToArray();

    public EnemyRegionConfig GetEnemyRegion(float height)
    {
        return EnemyRegions
            .OrderBy(region => Mathf.Abs(region.StartHeight - height))
            .FirstOrDefault();
    }

}
