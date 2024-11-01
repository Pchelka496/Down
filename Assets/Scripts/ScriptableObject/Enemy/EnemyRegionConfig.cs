using UnityEngine;
using static EnemyConfig;

[CreateAssetMenu(fileName = "EnemyRegionConfig", menuName = "Scriptable Objects/EnemyRegionConfig")]
public class EnemyRegionConfig : ScriptableObject
{
    [SerializeField] EnemyConfig[] _enemies;
    [SerializeField] float _startHeight;

    public Enemy[] Enemies
    {
        get
        {
            Enemy[] enemiesArray = new Enemy[_enemies.Length];

            for (int i = 0; i < _enemies.Length; i++)
            {
                enemiesArray[i] = _enemies[i].GetEnemy;
            }

            return enemiesArray;
        }
    }

    public float StartHeight { get => _startHeight; }

}
