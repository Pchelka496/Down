using System.Collections.Generic;
using Core;
using Core.Installers;
using Creatures.Player.Any;
using Creatures.Player.PlayerModule.CoreModules.EngineModule;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Creatures.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(HealthModule))]
    public class PlayerController : MonoBehaviour
    {
        public const float DEFAULT_DRAG = 0.2f;
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

        EnumState _currentState;
        readonly List<BaseModule> _modules = new(3);
        event System.Action DisposeEvents;

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
        private void Construct(Controls controls, GlobalEventsManager globalEventsManager)
        {
            controls.Enable();

            globalEventsManager.SubscribeToRoundStarted(RoundStart);
            globalEventsManager.SubscribeToRoundEnded(RoundEnd);
            globalEventsManager.SubscribeToWarpStarted(WarpStart);
            var taskId = globalEventsManager.AddTransitionTask(ResetPositionAndVelocity, false);

            DisposeEvents += () => globalEventsManager?.UnsubscribeFromWarpStarted(WarpStart);
            DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
            DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
            DisposeEvents += () => globalEventsManager?.RemoveTransitionTask(taskId);
        }

        private void Awake()
        {
            _currentState = EnumState.Lobby;
            EnterLobbyMode();
            ResetPositionAndVelocity();

            RegisterModule(_healthModule);
            RegisterModule(_rotationModule);
            RegisterModule(_engineModule);
            RegisterModule(_pickerModule);
            RegisterModule(_warpEngineModule);
        }

        private void WarpStart() => SetState(EnumState.Warp);
        private void RoundStart() => SetState(EnumState.Gameplay);
        private void RoundEnd() => SetState(EnumState.Lobby);

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<>")]
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
                        $"Loaded module is not of type {typeof(T).Name}. " +
                        $"Actual type: {loadedModule?.GetType().Name ?? "null"}");
                    return null;
                }
            }

            return moduleForTest;
        }

        public void OpenUIPanel() => SetState(EnumState.OpenUIPanel);

        public void CloseUIPanel()
        {
            switch (_currentState)
            {
                case EnumState.OpenUIPanel:
                    {
                        SetState(EnumState.Lobby);
                        break;
                    }

            }
        }

        private void SetState(EnumState newState)
        {
            if (_currentState == newState) return;

            _currentState = newState;

            switch (newState)
            {
                case EnumState.Lobby:
                    {
                        EnterLobbyMode();
                        break;
                    }
                case EnumState.Gameplay:
                    {
                        EnterGameplayMode();
                        break;
                    }
                case EnumState.Warp:
                    {
                        EnterWarpMode();
                        break;
                    }
                case EnumState.OpenUIPanel:
                    {
                        EnterOpenUIPanelMode();
                        break;
                    }
                default:
                    {
                        Debug.LogError($"Unknown player state: {newState}");
                        break;
                    }
            }
        }

        private void EnterLobbyMode()
        {
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.None;

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

        private UniTask ResetPositionAndVelocity()
        {
            switch (_currentState)
            {
                case EnumState.Lobby:
                    {
                        transform.position = new Vector2(LevelManager.PLAYER_START_X_POSITION, LevelManager.PLAYER_START_Y_POSITION);
                        _rb.velocity = Vector2.zero;

                        break;
                    }
            }

            return UniTask.CompletedTask;
        }

        private void EnterGameplayMode()
        {
            _rb.gravityScale = 1f;
            _rb.constraints = RigidbodyConstraints2D.None;

            foreach (var module in _modules)
            {
                module.EnableModule();
            }
        }

        private void EnterWarpMode()
        {
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.None;

            foreach (var module in _modules)
            {
                module.DisableModule();
            }
        }

        private void EnterOpenUIPanelMode()
        {
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;

            foreach (var module in _modules)
            {
                module.DisableModule();
            }
        }

        private void OnDestroy()
        {
            _playerVisualPart.Dispose();
            DisposeEvents?.Invoke();
        }

        public enum EnumState
        {
            Lobby,
            Gameplay,
            Warp,
            OpenUIPanel
        }
    }
}
