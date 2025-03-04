using Core;
using Core.Enemy;
using Core.Installers;
using ScriptableObject.ModulesConfig;
using UnityEngine;
using Zenject;

namespace Creatures.Player
{
    public class HealthModule : BaseModule, IPlayerCollisionTracker
    {
        [SerializeField] float _cameraShakeTime = 0.4f;
        [SerializeField] SoundPlayerRandomPitch _soundPlayer;

        [SerializeField] int _maxHealth;
        [SerializeField] int _currentHealth;

        [SerializeField] int _repairKitNumberForRepair;
        [SerializeField] int _currentRepairKit;

        int _collisionQuantity;
        bool _workFlag = true;
        Vector2 _previousVelocity;

        Rigidbody2D _rb;
        RepairKitIndicator _repairKitIndicator;
        HealthIndicator _healthIndicator;

        EffectController _effectController;
        CameraFacade _camerasController;
        GlobalEventsManager _globalEventsManager;

        event System.Action OnPlayerTakeImpact;
        event System.Action DisposeEvents;
        event System.Action<int> OnPlayerCollision;

        private int MaxHealth
        {
            set
            {
                _maxHealth = value;
                _healthIndicator.Initialize(_maxHealth, _currentHealth);
            }
        }

        private int CurrentHealth
        {
            set
            {
                _currentHealth = value;
                _healthIndicator.UpdateHealth(_currentHealth);

                if (_currentHealth < 0)
                {
                    _globalEventsManager.PlayerDied();
                }
            }
        }

        private int CurrentRepairKit
        {
            set
            {
                _currentRepairKit = value;
                _repairKitIndicator.UpdateCurrentRepairKit(_currentRepairKit);
            }
        }

        private int RepairKitNumberForRepair
        {
            set
            {
                _repairKitNumberForRepair = value;
                _repairKitIndicator.Initialize(_repairKitNumberForRepair);
            }
        }

        [Inject]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
        private void Construct(HealthModuleConfig config,
                               EffectController effectController,
                               CameraFacade camerasController,
                               AudioSourcePool audioSourcePool,
                               RepairKitIndicator repairKitIndicator,
                               HealthIndicator healthIndicator,
                               PlayerController player,
                               GlobalEventsManager globalEventsManager)
        {
            _globalEventsManager = globalEventsManager;
            _effectController = effectController;
            _camerasController = camerasController;
            _soundPlayer.Initialize(audioSourcePool);
            _repairKitIndicator = repairKitIndicator;
            _healthIndicator = healthIndicator;
            _rb = player.Rb;

            globalEventsManager.SubscribeToRoundStarted(RoundStart);

            DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);

            UpdateCharacteristics(config);
        }

        private void RoundStart()
        {
            _collisionQuantity = 0;
            UpdateCharacteristics();
        }

        private void UpdateCharacteristics() =>
            UpdateCharacteristics(GameplaySceneInstaller.DiContainer.Resolve<HealthModuleConfig>());

        public void UpdateCharacteristics(HealthModuleConfig config)
        {
            MaxHealth = config.MaximumHealth;
            CurrentHealth = _maxHealth;

            RepairKitNumberForRepair = config.RepairKitNumberForRepair;
            CurrentRepairKit = 0;
        }

        public override void EnableModule()
        {
            _workFlag = true;
        }

        public override void DisableModule()
        {
            _workFlag = false;
        }

        private void FixedUpdate()
        {
            _previousVelocity = _rb.velocity;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!_workFlag) return;

            if (collision.gameObject.layer == EnemySystemCoordinator.ENEMY_LAYER_INDEX)
            {
                ApplyDamage(collision.contacts[0].point);

                if (_collisionQuantity != int.MaxValue)
                {
                    _collisionQuantity++;
                }

                OnPlayerCollision(_collisionQuantity);
            }
        }

        private void ApplyDamage(Vector2 point)
        {
            CurrentHealth = _currentHealth - 1 - (int)_previousVelocity.magnitude;

            OnPlayerTakeImpact?.Invoke();
            _effectController.PlayImpactEffect(point);
            _camerasController.EnableCameraShake(_cameraShakeTime);
            _soundPlayer.PlayNextSound();
        }

        public void ApplyDamage(int damage)
        {
            CurrentHealth = _currentHealth - damage;
        }

        public void TestDealDamage(int damage)
        {
            CurrentHealth = Mathf.Clamp(_currentHealth - damage, 1, _maxHealth);
        }

        public void ApplyRepairKit()
        {
            CurrentRepairKit = _currentRepairKit + 1;

            if (_currentRepairKit >= _repairKitNumberForRepair)
            {
                HealthRegeneration();
            }
        }

        private void HealthRegeneration()
        {
            CurrentRepairKit = 0;
            CurrentHealth = _maxHealth;
        }

        public void SubscribeToOnPlayerTakeImpact(System.Action callback) => OnPlayerTakeImpact += callback;
        public void UnsubscribeFromOnPlayerTakeImpact(System.Action callback) => OnPlayerTakeImpact -= callback;

        int IPlayerCollisionTracker.GetCollisionCount() => _collisionQuantity;
        void IPlayerCollisionTracker.SubscribeToCollisionChanged(System.Action<int> callback) => OnPlayerCollision += callback;
        void IPlayerCollisionTracker.UnsubscribeFromCollisionChanged(System.Action<int> callback) => OnPlayerCollision -= callback;

        private void OnDestroy()
        {
            DisposeEvents?.Invoke();
        }
    }
}
