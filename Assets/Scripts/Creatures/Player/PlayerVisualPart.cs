using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Creatures.Player
{
    [System.Serializable]
    public class PlayerVisualPart : IHaveSkin, System.IDisposable
    {
        [SerializeField] Transform _skinParentObject;
        AssetReference _skinReference;
        AsyncOperationHandle<GameObject> _skinLoadHandle;
        GameObject _skin;

        public AssetReference Skin
        {
            get => _skinReference;
            set
            {
                if (value == null) return;

                if (_skinReference != null)
                {
                    if (_skinReference.AssetGUID == value.AssetGUID) return;
                }

                UpdateSkin(value).Forget();
                _skinReference = value;
            }
        }

        private async UniTaskVoid UpdateSkin(AssetReference newReference)
        {
            var loadData = await SkinPrefabLoad(newReference);

            ClearCurrentSkin();

            _skinLoadHandle = loadData.Handle;
            _skin = Object.Instantiate(loadData.LoadAsset, _skinParentObject);
        }

        private async UniTask<AddressableLouderHelper.LoadOperationData<GameObject>> SkinPrefabLoad(
            AssetReference reference)
        {
            return await AddressableLouderHelper.LoadAssetAsync<GameObject>(reference);
        }

        private void ClearCurrentSkin()
        {
            if (_skin != null)
            {
                Object.Destroy(_skin);
            }

            if (_skinLoadHandle.IsValid())
            {
                if (_skinLoadHandle is { IsDone: true, Status: AsyncOperationStatus.Succeeded })
                {
                    Addressables.Release(_skinLoadHandle);
                }
            }
        }

        public void Dispose()
        {
            ClearCurrentSkin();
        }
    }
}