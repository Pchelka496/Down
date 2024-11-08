using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Scriptable Objects/EnemyConfig")]
public class EnemyConfig : ScriptableObject
{
    [SerializeField] Enemy _enemy;

    public Enemy GetEnemy { get => _enemy; }

    [System.Serializable]
    public struct Enemy
    {
        [SerializeField] public AssetReference EnemyAddress;
        [SerializeField] public float Speed;
        [SerializeField] public EnumMotionPattern MotionPattern;
        [SerializeField] public float2 MotionCharacteristic;
        [SerializeField] public bool NeedUpdateRewards;
        [SerializeField] public Vector2 IsolateDistance;

        [Header("Free number, but the total cannot exceed the number of all enemies")]
        [SerializeField] public int RequiredAmount;
        [Header("Depends on the maximum enemy number")]
        [Tooltip("Depends on the residual number of enemies that have been spawned from all enemies and the mandatory number of enemies. AllEnemy - RequiredAmount = distribution quantity")]
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
