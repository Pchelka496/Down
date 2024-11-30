using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class CameraFacade : MonoBehaviour
{
    [SerializeField] CinemachineCamera _camera;

    [SerializeField] CameraShakerController _cameraShaker;
    [SerializeField] LensController _lensController;
    [SerializeField] PositionComposerController _positionComposerController;

    [SerializeField] Transform _cameraDefaultTrackingTarget;
    PlayerController _player;

    [Inject]
    public void Construct(LevelManager levelManager, PlayerController player)
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

        _positionComposerController.SetLobbyMode();
        _lensController.SetLobbyMode();
    }

    private void SetFlyMode()
    {
        SetTrackingTarget(_player.transform);

        _lensController.SetFlyMode();
        _positionComposerController.SetFlyMode();
    }

    private void SetTrackingTarget(Transform transform)
    {
        var target = new CameraTarget
        {
            TrackingTarget = transform
        };

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
        _cameraShaker?.Dispose();
    }

}
