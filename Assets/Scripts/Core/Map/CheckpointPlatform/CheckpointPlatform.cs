using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

public class CheckpointPlatform : MonoBehaviour
{
    [SerializeField] Canvas _canvas;
    [SerializeField] DisplayController _displayController;
    CheckpointPlatformController _platformController;
    CharacterController _player;
    MapController _mapController;
    LevelManager _levelManager;

    [Inject]
    private void Construct(MapController mapController, LevelManager levelManager, CharacterController player, Camera camera)
    {
        _mapController = mapController;
        _levelManager = levelManager;
        _player = player;
        _canvas.worldCamera = camera;
    }

    public void Initialize(Initializer initializer, CheckpointPlatformController platformController)
    {
        transform.position = new(0f, initializer.PlatformHeight);
        _platformController = platformController;
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == _player.gameObject)
        {
            _levelManager.RoundStart();
        }
    }

    public async void SetClimbingMode()
    {
        _player.transform.SetParent(transform);
        _displayController.SetClimbingMode().Forget();

        await UniTask.WaitForSeconds(1f);
        await _platformController.MovePlatformToHeight(_mapController.GetClosestPlatformAboveHeight(transform.position.y).PlatformHeight);
    }

    public async void SetDescendingMode()
    {
        _player.transform.SetParent(transform);
        _displayController.SetDescendingMode().Forget();

        await UniTask.WaitForSeconds(1f);
        await _platformController.MovePlatformToHeight(_mapController.GetClosestPlatformBelowHeight(transform.position.y).PlatformHeight);
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

