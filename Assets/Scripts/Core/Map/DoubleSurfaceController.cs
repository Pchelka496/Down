using Additional;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DoubleSurfaceController : MonoBehaviour
{
    [SerializeField] AssetReference _firstSurfacePrefab;
    [SerializeField] AssetReference _secondSurfacePrefab;

    GameObject _firstSurface;
    GameObject _secondSurface;

    AsyncOperationHandle<GameObject> _firstSurfaceLoadHandler;
    AsyncOperationHandle<GameObject> _secondSurfaceLoadHandler;

    CancellationTokenSource _cts;

    event System.Action DisposeEvents;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(Controls controls, GlobalEventsManager globalEventsManager)
    {
        controls.Enable();

        globalEventsManager.SubscribeToRoundStarted(RoundStart);
        globalEventsManager.SubscribeToRoundEnded(RoundEnd);
        globalEventsManager.SubscribeToWarpStarted(WarpStart);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromWarpStarted(WarpStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
    }

    private void WarpStart() => Destroy(this);
    private void RoundStart() => Destroy(this);
    private void RoundEnd() => Destroy(this);

    private async UniTask CreateSurface()
    {
        var firstSurfaceLoadData = await SurfacePrefabLoad(_firstSurfacePrefab);
        var secondSurfaceLoadData = await SurfacePrefabLoad(_secondSurfacePrefab);

        _firstSurface = Instantiate(firstSurfaceLoadData.LoadAsset);
        _firstSurfaceLoadHandler = firstSurfaceLoadData.Handle;

        _secondSurface = Instantiate(secondSurfaceLoadData.LoadAsset);
        _secondSurfaceLoadHandler = secondSurfaceLoadData.Handle;
    }

    private async UniTask<AddressableLouderHelper.LoadOperationData<GameObject>> SurfacePrefabLoad(AssetReference reference)
    {
        return await AddressableLouderHelper.LoadAssetAsync<GameObject>(reference);
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
        ClearToken();
        ClearLoadedSurface();
    }
}
