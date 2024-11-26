using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(HealthModule))]
public class CharacterController : MonoBehaviour
{
    public const float PLAYER_RADIUS = 0.7f;

    [SerializeField] Transform _bodySprite;
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] CircleCollider2D _collider;
    [SerializeField] MultiTargetRotationFollower _follower;

    [SerializeField] HealthModule _healthModule;
    [SerializeField] RotationModule _rotationModule;
    [SerializeField] EngineModule _engineModule;
    [SerializeField] PickerModule _pickerModule;

    readonly List<BaseModule> _modules = new(3);

    public Rigidbody2D Rb { get => _rb; set => _rb = value; }
    public CircleCollider2D Collider { get => _collider; set => _collider = value; }
    public MultiTargetRotationFollower MultiTargetRotationFollower => _follower;

    public HealthModule HealthModule { get => _healthModule; }
    public RotationModule RotationModule { get => _rotationModule; }
    public EngineModule EngineModule { get => _engineModule; }
    public PickerModule PickerModule { get => _pickerModule; }

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:”далите неиспользуемые закрытые члены", Justification = "<ќжидание>")]
    private void Construct(Controls controls, LevelManager levelManager)
    {
        controls.Enable();
        SetLobbyMode();
        levelManager.SubscribeToRoundStart(RoundStart);

        RegisterModule(_healthModule);
        RegisterModule(_rotationModule);
        RegisterModule(_engineModule);
        RegisterModule(_pickerModule);
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<ќжидание>")]
    public async UniTask<T> GetModuleForTest<T>() where T : BaseModule
    {
        var moduleForTest = GetModule<T>();

        if (moduleForTest == null)
        {
            var loadedModule = await GameplaySceneInstaller.DiContainer
                                    .Resolve<OptionalPlayerModuleLoader>()
                                    .LoadModuleForTest<T>();

            moduleForTest = loadedModule as T;

            if (moduleForTest == null)
            {
                Debug.LogError($"Loaded module is not of type {typeof(T).Name}. Actual type: {loadedModule?.GetType().Name ?? "null"}");
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

    public T GetModule<T>() where T : BaseModule
    {
        foreach (var module in _modules)
        {
            if (module is T typedModule)
            {
                return typedModule;
            }
        }
        return null;
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

}

