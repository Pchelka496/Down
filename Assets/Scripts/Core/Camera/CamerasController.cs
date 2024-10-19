using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class CamerasController : MonoBehaviour
{
    [SerializeField] CinemachineCamera _camera;
    //   [SerializeField] CinemachinePositionComposer _composer;
    //  [SerializeField] CameraCharacteristic _flyMode;
    // [SerializeField] CameraCharacteristic _checkpointPlatformMode;
    [SerializeField] CameraShaker _cameraShaker;
    [SerializeField] LensController _lensController;
    [SerializeField] PositionComposerController _positionComposerController;

    [Inject]
    public void Construct(LevelManager levelManager, CharacterController player)
    {
        _lensController.Initialize(_camera, player.Rb);
        _positionComposerController.Initialize(player.Rb);

        SetCheckpointPlatformMode();
        levelManager.SubscribeToRoundStart(RoundStart);
    }

    private void RoundStart(LevelManager levelManager)
    {
        SetFlyMode();
    }

    public void SetFlyMode()
    {
        //  ApplyCameraCharacteristics(_flyMode);
    }

    public void SetCheckpointPlatformMode()
    {
        //  ApplyCameraCharacteristics(_checkpointPlatformMode);
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

    //private void ApplyCameraCharacteristics(CameraCharacteristic characteristic)
    //{
    //    _composer.Lookahead = characteristic.LookHead;
    //    _composer.Composition = characteristic.ComposerSettings;
    //}

    //[System.Serializable]
    //private record CameraCharacteristic
    //{
    //    [field: SerializeField] public ScreenComposerSettings ComposerSettings { get; private set; }
    //    [field: SerializeField] public LookaheadSettings LookHead { get; private set; }

    //}

}
