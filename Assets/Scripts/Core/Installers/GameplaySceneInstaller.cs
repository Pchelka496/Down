using UnityEngine;
using Zenject;

public class GameplaySceneInstaller : MonoInstaller
{
    public static DiContainer DiContainer { get; private set; }

    public override void InstallBindings()
    {
        InitializeOrCleanInstaller();

        //Container.Bind<Controls>().FromNew().AsSingle().NonLazy();

        //Container.Bind<NetworkManager>().FromInstance(NetworkManager.singleton).AsSingle().NonLazy();
        //Container.Bind<BaseRoundManager>().FromInstance(_roundManager).AsSingle().NonLazy();
        //Container.Bind<CamerasController>().FromInstance(_camerasController).AsSingle().NonLazy();
        //Container.Bind<PlayerConnectionPanel>().FromInstance(_playerConnectionPanel).AsSingle().NonLazy();
        //Container.Bind<CombatHUD>().FromInstance(_combatHUDPanel).AsSingle().NonLazy();
        //Container.Bind<ItemStore>().FromInstance(_itemStore).AsSingle().NonLazy();
        //Container.Bind<PlayerProfileConfig>().FromInstance(_playerProfileConfig).AsSingle().NonLazy();
        //Container.Bind<GameplaySceneConfig>().FromInstance(_gameplaySceneConfig).AsSingle().NonLazy();
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

