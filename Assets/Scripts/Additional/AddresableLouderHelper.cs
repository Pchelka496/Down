using Cysharp.Threading.Tasks;
using System.Net;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressableLouderHelper
{
    public static async UniTask<LoadOperationData<T>> LoadAssetAsync<T>(AssetReference reference)
    {
        if (reference == null)
        {
            Debug.LogError("Invalid AssetReference");
            return default;
        }

        var handle = Addressables.LoadAssetAsync<T>(reference);

        await handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return new(handle, handle.Result);
        }
        else
        {
            Debug.LogError($"Error loading via Addressable. GUID - {reference.AssetGUID}");
            return default;
        }
    }

    public static async UniTask<LoadOperationData<T>> LoadAssetAsync<T>(string address)
    {
        if (address == null)
        {
            Debug.LogError("Invalid address");
            return default;
        }

        var handle = Addressables.LoadAssetAsync<T>(address);

        await handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return new(handle, handle.Result);
        }
        else
        {
            Debug.LogError($"Error loading via Addressable. Asset Address - {address}");
            return default;
        }
    }

    public readonly struct LoadOperationData<T>
    {
        public readonly AsyncOperationHandle<T> Handle;
        public readonly T LoadAsset;

        public LoadOperationData(AsyncOperationHandle<T> handle, T loadAsset)
        {
            Handle = handle;
            LoadAsset = loadAsset;
        }
    }

}
