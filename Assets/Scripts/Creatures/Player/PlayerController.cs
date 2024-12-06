using System.Collections.Generic;
using Core;
using Core.Installers;
using Creatures.Player.Any;
using Creatures.Player.FlightModule.EngineModule;
using Creatures.Player.PlayerModule.CoreModules.EngineModule;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Creatures.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(HealthModule))]
    public class PlayerController : MonoBehaviour
    {
        public const float PLAYER_RADIUS = 0.7f;

        [SerializeField] Rigidbody2D _rb;
        [SerializeField] CircleCollider2D _collider;
        [SerializeField] MultiTargetRotationFollower _follower;

        [SerializeField] HealthModule _healthModule;
        [SerializeField] RotationModule _rotationModule;
        [SerializeField] EngineModule _engineModule;
        [SerializeField] PickerModule _pickerModule;
        [SerializeField] WarpEngineModule _warpEngineModule;
        [SerializeField] PlayerVisualPart _playerVisualPart;

        readonly List<BaseModule> _modules = new(3);

        public Rigidbody2D Rb => _rb;
        public CircleCollider2D Collider => _collider;

        public HealthModule HealthModule => _healthModule;
        public RotationModule RotationModule => _rotationModule;
        public EngineModule EngineModule => _engineModule;
        public PickerModule PickerModule => _pickerModule;
        public WarpEngineModule WarpEngineModule => _warpEngineModule;

        public MultiTargetRotationFollower MultiTargetRotationFollower => _follower;

        public PlayerVisualPart PlayerVisualPart => _playerVisualPart;

        [Inject]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:������� �������������� �������� �����",
            Justification = "<��������>")]
        private void Construct(Controls controls, LevelManager levelManager)
        {
            controls.Enable();
            SetLobbyMode();
            levelManager.SubscribeToRoundStart(RoundStart);

            RegisterModule(_healthModule);
            RegisterModule(_rotationModule);
            RegisterModule(_engineModule);
            RegisterModule(_pickerModule);
            RegisterModule(_warpEngineModule);
        }

        private void RoundStart(LevelManager levelManager)
        {
            levelManager.SubscribeToRoundEnd(RoundEnd);
            SetGameplayMode();
        }

        private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
        {
            levelManager.SubscribeToRoundStart(RoundStart);
            SetLobbyMode();
        }

        private void SetLobbyMode()
        {
            _rb.gravityScale = 0f;

            foreach (var module in _modules)
            {
                if (!module.IsActiveOnLobbyMode)
                {
                    module.DisableModule();
                }
                else
                {
                    module.EnableModule();
                }
            }
        }

        private void SetGameplayMode()
        {
            _rb.gravityScale = 1f;

            foreach (var module in _modules)
            {
                module.EnableModule();
            }
        }

        public void OpenPanel()
        {
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
            DisableModulesInLobbyMode();
        }

        public void ClosePanel()
        {
            _rb.constraints = RigidbodyConstraints2D.None;
            EnableModulesInLobbyMode();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects",
            Justification = "<��������>")]
        public async UniTask<T> GetModuleForTest<T>() where T : BaseModule
        {
            if (!GetModule<T>(out var moduleForTest))
            {
                var loadedModule = await GameplaySceneInstaller.DiContainer
                    .Resolve<OptionalPlayerModuleLoader>()
                    .LoadModuleForTest<T>();

                moduleForTest = loadedModule as T;

                if (moduleForTest == null)
                {
                    Debug.LogError(
                        $"Loaded module is not of type {typeof(T).Name}. Actual type: {loadedModule?.GetType().Name ?? "null"}");
                    return null;
                }
            }

            return moduleForTest;
        }

        public void RegisterModule(BaseModule module)
        {
            if (!_modules.Contains(module))
            {
                _modules.Add(module);
            }
        }

        public bool GetModule<T>(out T module) where T : BaseModule
        {
            foreach (var playerModule in _modules)
            {
                if (playerModule is T typedModule)
                {
                    module = typedModule;
                    return true;
                }
            }

            module = null;
            return false;
        }

        private void EnableModulesInLobbyMode()
        {
            foreach (var module in _modules)
            {
                if (module.IsActiveOnLobbyMode)
                {
                    module.EnableModule();
                }
            }
        }

        private void DisableModulesInLobbyMode()
        {
            foreach (var module in _modules)
            {
                if (module.IsActiveOnLobbyMode)
                {
                    module.DisableModule();
                }
            }
        }

        private void OnDestroy()
        {
            _playerVisualPart.Dispose();
        }
    }
}