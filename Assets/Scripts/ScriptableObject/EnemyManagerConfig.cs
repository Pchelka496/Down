using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Unity.Mathematics;

[CreateAssetMenu(fileName = "EnemyManagerConfig", menuName = "Scriptable Objects/EnemyManagerConfig")]
public class EnemyManagerConfig : ScriptableObject
{
    [SerializeField] string _enemyCorePrefab;
    [SerializeField] int _enemyCount;
    [SerializeField] EnemyRegion[] _enemyRegions;

    public int EnemyCount { get => _enemyCount; set => _enemyCount = value; }
    public string EnemyCorePrefab { get => _enemyCorePrefab; set => _enemyCorePrefab = value; }
    public Stack<EnemyRegion> EnemyRegions
    {
        get
        {
            return new Stack<EnemyRegion>(_enemyRegions.OrderBy(region => region.StartHeight));
        }
    }

    public EnemyRegion GetEnemyRegion(float height)
    {
        return EnemyRegions
            .OrderBy(region => Mathf.Abs(region.StartHeight - height))
            .FirstOrDefault();
    }

    [System.Serializable]
    public record EnemyRegion
    {
        [SerializeField] Enemy[] _enemies;
        [SerializeField] float _startHeight;

        public Enemy[] Enemies { get => _enemies; }
        public float StartHeight { get => _startHeight; }

    }

    [System.Serializable]
    public struct Enemy
    {
        [SerializeField] public string EnemyAddress;
        [SerializeField] public float Speed;
        [SerializeField] public EnumMotionPattern MotionPattern;
        [SerializeField] public float2 MotionCharacteristic;

        [SerializeField][Range(0f, 1f)] public float RelativeAmount;

        public static Enemy EmptyEnemy()
        {
            return new Enemy
            {
                EnemyAddress = null,
                Speed = 0f,
                MotionPattern = EnumMotionPattern.Static,
                MotionCharacteristic = 0f
            };
        }

    }

}
