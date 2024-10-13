using UnityEngine;
using Zenject;

public class GameplaySceneInstaller : MonoInstaller
{
    [SerializeField] CharacterController _playerController;
    [SerializeField] LevelManager _levelManager;
    [SerializeField] EnemyManager _enemyManager;
    [SerializeField] MapController _mapController;
    [SerializeField] BackgroundController _backgroundController;
    [SerializeField] Camera _mainCamera;
    [SerializeField] PointKeeperConfig _pointKeeperConfig;
    [SerializeField] EnumLanguage _enumLanguage;

    public static DiContainer DiContainer { get; private set; }

    public override void InstallBindings()
    {
        InitializeOrCleanInstaller();
        Container.Bind<Controls>().FromNew().AsSingle().NonLazy();

        Container.Bind<CharacterController>().FromInstance(_playerController).AsSingle().NonLazy();
        Container.Bind<MapController>().FromInstance(_mapController).AsSingle().NonLazy();
        Container.Bind<BackgroundController>().FromInstance(_backgroundController).AsSingle().NonLazy();
        Container.Bind<EnemyManager>().FromInstance(_enemyManager).AsSingle().NonLazy();
        Container.Bind<LevelManager>().FromInstance(_levelManager).AsSingle().NonLazy();
        Container.Bind<Camera>().FromInstance(_mainCamera).AsSingle().NonLazy();
        Container.Bind<EnumLanguage>().FromInstance(_enumLanguage).AsSingle().NonLazy();

        var pointKeeper = new PointKeeper();
        pointKeeper.Initialize(_pointKeeperConfig);
        Container.Bind<PointKeeper>().FromInstance(pointKeeper).AsSingle().NonLazy();
    }

    private void InitializeOrCleanInstaller()
    {
        if (DiContainer == null)
        {
            DiContainer = Container;
        }
        else
        {
            if (gameObject.TryGetComponent<GameplaySceneInstaller>(out var thisComponent))
            {
                Destroy(thisComponent);
            }
        }
    }

}

