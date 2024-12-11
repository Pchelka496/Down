using Core;
using Creatures.Player;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using DG.Tweening;
using System;

public class CameraFacade : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] CinemachineCamera _cinemachineCamera;

    [SerializeField] CameraShakerController _cameraShaker;
    [SerializeField] LensController _lensController;
    [SerializeField] PositionComposerController _positionComposerController;

    [SerializeField] Transform _cameraDefaultTrackingTarget;

    PlayerController _player;
    CameraState _currentState;

    bool _onChangeTargetAnimationEnding;

    public Camera Camera => _camera;
    public float LobbyOrthographicSize => _lensController.LobbyModeOrthographicSize;
    public bool OnChangeTargetAnimationEnding { get => _onChangeTargetAnimationEnding; }

    Func<bool> _isTransitioning = () => false;

    event Action DisposeEvents;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(GlobalEventsManager globalEventsManager, PlayerController player)
    {
        _player = player;

        globalEventsManager.SubscribeToRoundStarted(RoundStart);
        globalEventsManager.SubscribeToRoundEnded(RoundEnd);
        globalEventsManager.SubscribeToWarpStarted(WarpStart);

        _isTransitioning = () => globalEventsManager != null && globalEventsManager.IsTransitioning;

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromWarpStarted(WarpStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
    }

    private void Awake()
    {
        _cameraDefaultTrackingTarget.position = new(LevelManager.PLAYER_START_X_POSITION, LevelManager.PLAYER_START_Y_POSITION);

        _lensController.Initialize(_cinemachineCamera, _player.Rb);
        _positionComposerController.Initialize(_player.Rb);

        _currentState = CameraState.Lobby;

        EnterLobbyMode().Forget();
    }

    private void WarpStart() => SetState(CameraState.Warp).Forget();
    private void RoundStart() => SetState(CameraState.Gameplay).Forget();
    private void RoundEnd() => SetState(CameraState.Lobby).Forget();

    private async UniTask SetState(CameraState newState)
    {
        if (_currentState == newState) return;

        _currentState = newState;

        switch (newState)
        {
            case CameraState.Lobby:
                {
                    await EnterLobbyMode();
                    break;
                }
            case CameraState.Gameplay:
                {
                    await EnterGameplayMode();
                    break;
                }
            case CameraState.Warp:
                {
                    await EnterWarpMode();
                    break;
                }
            default:
                {
                    Debug.LogError($"Unknown camera state: {newState}");
                    break;
                }
        }
    }

    private async UniTask EnterLobbyMode()
    {
        await UniTask.WaitUntil(() => !_isTransitioning.Invoke());
        await SetTrackingTarget(_cameraDefaultTrackingTarget);

        _positionComposerController.SetState(PositionComposerController.ComposerState.Lobby);
        _lensController.SetState(LensController.LensState.Lobby);
    }

    private async UniTask EnterGameplayMode()
    {
        const float DURATION = 1f;

        await SetTrackingTarget(_player.transform, DURATION, AnimationCurve.EaseInOut(0f, 0f, 1f, 1f));

        _positionComposerController.SetState(PositionComposerController.ComposerState.Gameplay);
        _lensController.SetState(LensController.LensState.Gameplay);
    }

    private async UniTask EnterWarpMode()
    {
        const float DURATION = 1f;

        await SetTrackingTarget(_player.transform, DURATION, AnimationCurve.EaseInOut(0f, 0f, 1f, 1f));

        _positionComposerController.SetState(PositionComposerController.ComposerState.Warp);
        _lensController.SetState(LensController.LensState.Warp);
    }

    private async UniTask SetTrackingTarget(Transform target, float? duration = null, AnimationCurve curve = null)
    {
        _cinemachineCamera.Target = new CameraTarget();

        if (target == null) return;

        _onChangeTargetAnimationEnding = false;

        if (duration.HasValue && curve != null)
        {
            await _cinemachineCamera.transform.DOMove(target.position, duration.Value)
                                   .SetEase(curve)
                                   .AsyncWaitForCompletion();
        }
        else
        {
            _cinemachineCamera.transform.position = target.position;
        }

        var cameraTarget = new CameraTarget
        {
            TrackingTarget = target
        };

        _cinemachineCamera.Target = cameraTarget;

        _onChangeTargetAnimationEnding = true;
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

        DisposeEvents?.Invoke();
        DisposeEvents = null;
    }

    public enum CameraState
    {
        Lobby,
        Gameplay,
        Warp
    }
}
