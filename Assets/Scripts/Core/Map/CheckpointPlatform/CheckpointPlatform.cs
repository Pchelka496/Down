using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class CheckpointPlatform : MonoBehaviour
{
    const float START_MOVEMENT_DELAY = 2f;
    const float CAMERA_SHAKE_DELAY = 0.5f;

    [SerializeField] Canvas _canvas;
    [SerializeField] DisplayController _displayController;

    CheckpointPlatformController _platformController;
    CharacterController _player;
    MapController _mapController;
    LevelManager _levelManager;
    CamerasController _camerasController;

    [Inject]
    private void Construct(MapController mapController, LevelManager levelManager, CharacterController player, Camera camera, CamerasController camerasController)
    {
        _mapController = mapController;
        _levelManager = levelManager;
        _player = player;
        _canvas.worldCamera = camera;
        _camerasController = camerasController;
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
        _displayController.SetClimbingMode(CheckpointPlatformController.PLATFORM_MOVE_DURATION).Forget();

        await UniTask.WaitForSeconds(START_MOVEMENT_DELAY);
        _platformController.MovePlatformToHeight(_mapController.GetClosestPlatformAboveHeight(transform.position.y).PlatformHeight).Forget();

        await UniTask.WaitForSeconds(CAMERA_SHAKE_DELAY);
        _camerasController.EnableCameraShake(CheckpointPlatformController.PLATFORM_MOVE_DURATION - CAMERA_SHAKE_DELAY);
    }

    public async void SetDescendingMode()
    {
        _player.transform.SetParent(transform);
        _displayController.SetDescendingMode(CheckpointPlatformController.PLATFORM_MOVE_DURATION).Forget();

        await UniTask.WaitForSeconds(START_MOVEMENT_DELAY);
        _platformController.MovePlatformToHeight(_mapController.GetClosestPlatformBelowHeight(transform.position.y).PlatformHeight).Forget();

        await UniTask.WaitForSeconds(CAMERA_SHAKE_DELAY);
        _camerasController.EnableCameraShake(CheckpointPlatformController.PLATFORM_MOVE_DURATION - CAMERA_SHAKE_DELAY);
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

