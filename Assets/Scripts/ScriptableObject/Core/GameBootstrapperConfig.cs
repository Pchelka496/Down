using ScriptableObject.Enemy;
using ScriptableObject.Map;
using ScriptableObject.PickUpItem;
using UnityEngine;

namespace ScriptableObject
{
    [CreateAssetMenu(fileName = "GameBootstrapperConfig", menuName = "Scriptable Objects/GameBootstrapperConfig")]
    public class GameBootstrapperConfig : UnityEngine.ScriptableObject
    {
        [SerializeField] int _targetFrameRate = 90;
        [SerializeField] EnemyManagerConfig _enemyManagerConfig;
        [SerializeField] MapControllerConfig _mapControllerConfig;
        [SerializeField] BackgroundControllerConfig _backgroundControllerConfig;
        [SerializeField] PickUpItemManagerConfig _pickUpItemManagerConfig;

        public EnemyManagerConfig EnemyManagerConfig => _enemyManagerConfig;
        public MapControllerConfig MapControllerConfig => _mapControllerConfig;
        public BackgroundControllerConfig BackgroundControllerConfig => _backgroundControllerConfig;
        public PickUpItemManagerConfig RewardManagerConfig { get => _pickUpItemManagerConfig; set => _pickUpItemManagerConfig = value; }
        public int TargetFrameRate => _targetFrameRate;
    }
}
