using UnityEngine;
using Zenject;

public class CheckpointPlatform : MonoBehaviour
{
    MapController _mapController;
    LevelManager _levelManager;

    [Inject]
    private void Construct(MapController mapController, LevelManager levelManager)
    {
        _mapController = mapController;
        _levelManager = levelManager;
    }

    public void Initialize(Initializer initializer)
    {
        transform.position = new(0f, initializer.PlatformHeight);

    }

    private void PlatformDeactivation()
    {
     
    }

    public readonly struct Initializer
    {
        public readonly float PlatformHeight;

        public Initializer(float platformHeight)
        {
            PlatformHeight = platformHeight;
        }

    }

}

