using UnityEngine;
using Zenject;

public class CheckpointPlatform : MonoBehaviour
{
    CharacterController _player;
    MapController _mapController;
    LevelManager _levelManager;

    [Inject]
    private void Construct(MapController mapController, LevelManager levelManager, CharacterController player)
    {
        _mapController = mapController;
        _levelManager = levelManager;
        _player = player;
    }

    public void Initialize(Initializer initializer)
    {
        transform.position = new(0f, initializer.PlatformHeight);

    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == _player.gameObject)
        {
            _levelManager.RoundStart();
        }
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

