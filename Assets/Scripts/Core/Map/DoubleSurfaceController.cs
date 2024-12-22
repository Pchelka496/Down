using Additional;
using Creatures.Player;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DoubleSurfaceController : MonoBehaviour
{
    const float SURFACE_Y_POSITION = 0 - PlayerController.PLAYER_RADIUS;

    [SerializeField] AssetReference _firstSurfacePrefab;
    [SerializeField] AssetReference _secondSurfacePrefab;

    [SerializeField] Vector2 _surfaceSize;

    GameObject _firstSurface;
    GameObject _secondSurface;
    GameObject _player;
    GlobalEventsManager _globalEventsManager;

    AsyncOperationHandle<GameObject> _firstSurfaceLoadHandler;
    AsyncOperationHandle<GameObject> _secondSurfaceLoadHandler;

    CancellationTokenSource _cts;

    event System.Action DisposeEvents;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(GlobalEventsManager globalEventsManager, PlayerController playerController)
    {
        _player = playerController.gameObject;
        _globalEventsManager = globalEventsManager;

        globalEventsManager.SubscribeToRoundStarted(RoundStart);
        globalEventsManager.SubscribeToRoundEnded(RoundEnd);
        globalEventsManager.SubscribeToFastTravelStarted(FastTravelStart);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromFastTravelStarted(FastTravelStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
    }

    private void FastTravelStart() => Destroy(this);
    private void RoundStart() => Destroy(this);
    private void RoundEnd() => Destroy(this);

    private void Start()
    {
        ClearToken();
        _cts = new();

        CreateSurface(_cts.Token).Forget();
    }

    private async UniTask CreateSurface(CancellationToken token)
    {
        var firstSurfaceLoadData = await SurfacePrefabLoad(_firstSurfacePrefab);
        var secondSurfaceLoadData = await SurfacePrefabLoad(_secondSurfacePrefab);

        _firstSurface = Instantiate(firstSurfaceLoadData.LoadAsset, transform);
        _firstSurfaceLoadHandler = firstSurfaceLoadData.Handle;

        _secondSurface = Instantiate(secondSurfaceLoadData.LoadAsset, transform);
        _secondSurfaceLoadHandler = secondSurfaceLoadData.Handle;

        PositionSurfaces();

        await TrackPlayerMovement(token);
    }

    private async UniTask<AddressableLouderHelper.LoadOperationData<GameObject>> SurfacePrefabLoad(AssetReference reference)
    {
        return await AddressableLouderHelper.LoadAssetAsync<GameObject>(reference);
    }

    private void PositionSurfaces()
    {
        float halfWidth = _surfaceSize.x / 2f;

        _firstSurface.transform.position = new(PlayerPositionMeter.XPosition - halfWidth, SURFACE_Y_POSITION);
        _secondSurface.transform.position = new(PlayerPositionMeter.XPosition + halfWidth, SURFACE_Y_POSITION);
    }

    private async UniTask TrackPlayerMovement(CancellationToken token)
    {
        float halfWidth = _surfaceSize.x;

        while (!token.IsCancellationRequested)
        {
            var playerX = PlayerPositionMeter.XPosition;

            if (playerX - _firstSurface.transform.position.x > halfWidth)
            {
                _firstSurface.transform.position = new Vector3(
                    _secondSurface.transform.position.x + _surfaceSize.x, SURFACE_Y_POSITION, 0);

                SwapSurfaces();
            }

            if (_secondSurface.transform.position.x - playerX > halfWidth)
            {
                _secondSurface.transform.position = new Vector3(
                    _firstSurface.transform.position.x - _surfaceSize.x, SURFACE_Y_POSITION, 0);

                SwapSurfaces();
            }

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken: token);
        }
    }

    private void SwapSurfaces()
    {
        (_secondSurface, _firstSurface) = (_firstSurface, _secondSurface);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == _player)
        {
            _globalEventsManager.PlayerReachedSurface();
        }
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _cts);

    private void ClearLoadedSurface()
    {
        if (_firstSurface != null)
        {
            Destroy(_firstSurface);
        }
        ClearLoadHandler(ref _firstSurfaceLoadHandler);

        if (_secondSurface != null)
        {
            Destroy(_secondSurface);
        }
        ClearLoadHandler(ref _secondSurfaceLoadHandler);
    }

    private void ClearLoadHandler(ref AsyncOperationHandle<GameObject> handle)
    {
        if (handle.IsValid() &&
            handle.Status == AsyncOperationStatus.Succeeded)
        {
            Addressables.Release(handle);
            handle = default;
        }
    }

    private void OnDestroy()
    {
        DisposeEvents?.Invoke();
        ClearToken();
        ClearLoadedSurface();
    }
}
