using UnityEngine;

[CreateAssetMenu(fileName = "EnemyManagerConfig", menuName = "Scriptable Objects/EnemyManagerConfig")]
public class EnemyManagerConfig : ScriptableObject
{
    [SerializeField] EnemyRegion[] _enemyRegions;

    [System.Serializable]
    public record EnemyRegion
    {
        [SerializeField] Enemy[] _enemies;
        [SerializeField] float _startHeight;

        public float StartHeight { get => _startHeight; }

    }

    public record Enemy
    {
        [SerializeField] string _enemyAddress;

        public string EnemyAddress { get => _enemyAddress; }

    }

}
