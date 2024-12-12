using UnityEngine;

namespace ScriptableObject.Enemy
{
    [CreateAssetMenu(fileName = "EnemyRegionConfig", menuName = "Scriptable Objects/EnemyRegionConfig")]
    public class EnemyRegionConfig : UnityEngine.ScriptableObject
    {
        [SerializeField] EnemyConfig[] _enemies;
        [SerializeField] float _startHeight;

        public EnemyConfig.Enemy[] Enemies
        {
            get
            {
                EnemyConfig.Enemy[] enemiesArray = new EnemyConfig.Enemy[_enemies.Length];

                for (int i = 0; i < _enemies.Length; i++)
                {
                    enemiesArray[i] = _enemies[i].GetEnemy;
                }

                return enemiesArray;
            }
        }

        public float StartHeight => _startHeight;
    }
}
