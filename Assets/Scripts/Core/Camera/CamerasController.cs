using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class CamerasController : MonoBehaviour
{
    [SerializeField] CinemachineCamera _camera;
    [SerializeField] CameraShaker _cameraShaker;
    [SerializeField] LensController _lensController;
    [SerializeField] PositionComposerController _positionComposerController;
    [SerializeField] Transform _cameraDefaultTrackingTarget;
    CharacterController _player;

    [Inject]
    public void Construct(LevelManager levelManager, CharacterController player)
    {
        _player = player;

        levelManager.SubscribeToRoundStart(RoundStart);

        _lensController.Initialize(_camera, _player.Rb);
        _positionComposerController.Initialize(_player.Rb);

        SetLobbyMod();
        _cameraDefaultTrackingTarget.position = new(LevelManager.PLAYER_START_X_POSITION, LevelManager.PLAYER_START_Y_POSITION);
    }

    private void RoundStart(LevelManager levelManager)
    {
        SetFlyMode();
        levelManager.SubscribeToRoundEnd(RoundEnd);
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
        SetLobbyMod();
    }

    private void SetLobbyMod()
    {
        SetTrackingTarget(_cameraDefaultTrackingTarget);

        _positionComposerController.SetLobbyMod();
        _lensController.SetLobbyMod();
    }

    private void SetFlyMode()
    {
        SetTrackingTarget(_player.transform);

        _lensController.SetFlyMode();
        _positionComposerController.SetLobbyMod();
    }

    private void SetTrackingTarget(Transform transform)
    {
        var target = new CameraTarget();

        target.TrackingTarget = transform;
        _camera.Target = target;
    }

    public void EnableCameraShake(float? time = null)
    {
        if (time != null)
        {
            _cameraShaker.StartShake(time).Forget();
        }
        else
        {
            _cameraShaker.StartShake();
        }
    }

    public void DisableCameraShake()
    {
        _cameraShaker.StopShake();
    }

    private void OnDestroy()
    {
        _lensController?.Dispose();
        _positionComposerController?.Dispose();
    }

}
