using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using DG.Tweening;
using Zenject;

public class ModulesTester : MonoBehaviour
{
    [SerializeField] AssetReference _meteoriteReference;
    [SerializeField] AssetReference _repairKitReference;

    [Header("HealthModuleTest")]
    [SerializeField] int _meteoritePoolSize = 5;
    [SerializeField] int _repairKitPoolSize = 5;
    [SerializeField] RectTransform _meteoriteStartPosition;
    [SerializeField] float _meteoriteLaunchForce = 500f;
    [SerializeField] float _minMeteoriteAngularVelosity = 100f;
    [SerializeField] float _maxMeteoriteAngularVelosity = -100f;

    [SerializeField] RectTransform _repairKitStartPosition;
    [SerializeField] float _movementDuration = 1f;

    [SerializeField] GameObject _attackButton;
    [SerializeField] GameObject _healingButton;

    [SerializeField] RectTransform _healthIndicatorPosition;
    [SerializeField] RectTransform _repairKitIndicatorPosition;

    readonly Queue<BaseModule> _testModulesQueue = new();
    CharacterController _player;

    readonly ObjectPool<Meteorite> _meteoritePool = new();
    readonly ObjectPool<RepairKit> _repairKitPool = new();

    RepairKitIndicator _repairKitIndicator;
    HealthIndicator _healthIndicator;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(CharacterController player, RepairKitIndicator repairKitIndicator, HealthIndicator healthIndicator)
    {
        _player = player;
        _healthIndicator = healthIndicator;
        _repairKitIndicator = repairKitIndicator;

        _attackButton.SetActive(false);
        _healingButton.SetActive(false);
    }

    public async UniTaskVoid ModuleTest<T>() where T : BaseModule
    {
        var moduleForTest = await _player.GetModuleForTest<T>();

        if (moduleForTest != null)
        {
            moduleForTest.EnableModule();
            _testModulesQueue.Enqueue(moduleForTest);
        }

        if (typeof(T) == typeof(HealthModule))
        {
            HealthModuleTest().Forget();
        }
    }

    private async UniTaskVoid HealthModuleTest()
    {
        await LoadTestAssets();

        _attackButton.SetActive(true);
        _healingButton.SetActive(true);

        _healthIndicator.SetNewIndicatorPosition(_healthIndicatorPosition.position);
        _repairKitIndicator.SetNewIndicatorPosition(_repairKitIndicatorPosition.position);

        _repairKitIndicator.gameObject.SetActive(true);
        _healthIndicator.gameObject.SetActive(true);
    }

    public void StopTest()
    {
        while (_testModulesQueue.Count > 0)
        {
            var module = _testModulesQueue.Dequeue();

            if (module != null)
            {
                module.DisableModule();
            }
        }

        _attackButton.SetActive(false);
        _healingButton.SetActive(false);

        _repairKitIndicator.gameObject.SetActive(false);
        _healthIndicator.gameObject.SetActive(false);

        _healthIndicator.SetDefaultIndicatorPosition();
        _repairKitIndicator.SetDefaultIndicatorPosition();

        UnloadTestAssets().Forget();
    }

    private async UniTask LoadTestAssets()
    {
        if (!_meteoritePool.IsObjectAvailable)
        {
            var prefab = await LoadAssetPrefab(_meteoriteReference);
            _meteoritePool.InitializePool(prefab, _meteoritePoolSize);
        }

        if (!_repairKitPool.IsObjectAvailable)
        {
            var prefab = await LoadAssetPrefab(_repairKitReference);
            _repairKitPool.InitializePool(prefab, _repairKitPoolSize);
        }
    }

    private async UniTask<GameObject> LoadAssetPrefab(AssetReference reference)
    {
        var handle = reference.LoadAssetAsync<GameObject>();

        await handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }
        else
        {
            Debug.LogError("Error loading via Addressable.");
            return default;
        }
    }

    private async UniTaskVoid UnloadTestAssets()
    {
        _meteoritePool.ReleaseAll();
        _repairKitPool.ReleaseAll();

        await UniTask.WaitForEndOfFrame();

        if (_meteoriteReference.OperationHandle.IsValid())
        {
            _meteoriteReference.ReleaseAsset();
        }
        if (_meteoriteReference.OperationHandle.IsValid())
        {
            _repairKitReference.ReleaseAsset();
        }
    }

    public void LaunchRepairKit()
    {
        if (_repairKitPool.TryGetObject(out var repairKit))
        {
            repairKit.Relocate(_repairKitStartPosition.position);
            MoveObject(repairKit.gameObject, _repairKitStartPosition, _player.transform.position);
            ReturnToPool(repairKit, _repairKitPool, 3f).Forget();
        }
        else
        {
            Debug.LogWarning("No available Repair Kits in the pool.");
        }
    }

    public void LaunchMeteorite()
    {
        if (_meteoritePool.TryGetObject(out var meteorite))
        {
            meteorite.Launch(_meteoriteStartPosition.position,
                             _meteoriteLaunchForce,
                             _player.transform.position,
                             Random.Range(_minMeteoriteAngularVelosity, _maxMeteoriteAngularVelosity));

            ReturnToPool(meteorite, _meteoritePool, 3f).Forget();
        }
        else
        {
            Debug.LogWarning("No available Meteorites in the pool.");
        }
    }

    private async UniTask ReturnToPool<T>(T @object, ObjectPool<T> pool, float delayTime) where T : MonoBehaviour
    {
        await UniTask.Delay((int)(delayTime * 1000));

        if (@object != null)
        {
            @object.gameObject.SetActive(false);

            pool.ReturnObject(@object);
        }
    }

    private void MoveObject(GameObject obj, RectTransform startPosition, Vector3 targetPosition)
    {
        obj.transform.position = startPosition.position;

        obj.transform
            .DOMove(targetPosition, _movementDuration)
            .SetEase(Ease.InOutQuad);
    }

    private void OnDestroy()
    {
        UnloadTestAssets().Forget();
    }

    private class ObjectPool<T> where T : MonoBehaviour
    {
        readonly Queue<T> _pool = new();
        GameObject _prefab;
        int _poolSize;

        public bool IsObjectAvailable => _pool.Count > 0;

        public void InitializePool(GameObject prefab, int size)
        {
            _prefab = prefab;
            _poolSize = size;

            var diContainer = GameplaySceneInstaller.DiContainer;

            for (int i = 0; i < _poolSize; i++)
            {
                var obj = diContainer.InstantiatePrefabForComponent<T>(_prefab);

                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        public bool TryGetObject(out T obj)
        {
            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
                obj.gameObject.SetActive(true);
                return true;
            }

            obj = null;
            return false;
        }

        public void ReturnObject(T obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }

        public void ReleaseAll()
        {
            while (_pool.Count > 0)
            {
                var obj = _pool.Dequeue();
                Object.Destroy(obj.gameObject);
            }
        }
    }

}




