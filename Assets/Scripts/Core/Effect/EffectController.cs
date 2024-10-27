using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EffectController : MonoBehaviour
{
    [SerializeField] string _impactEffectPrefabAddress;
    [Header("Pool size = _initialPoolSize + 1 prefab")]
    [SerializeField] int _initialPoolSize;
    Queue<ParticleSystem> _impactEffectPool = new();
    ParticleSystem _impactEffectPrefab;

    private async void Start()
    {
        _impactEffectPrefab = await LoadEffectPrefab(_impactEffectPrefabAddress);
        _impactEffectPool.Enqueue(_impactEffectPrefab);

        for (int i = 0; i < _initialPoolSize; i++)
        {
            _impactEffectPool.Enqueue(CreateNewEffect());
        }
    }

    private async UniTask<ParticleSystem> LoadEffectPrefab(string address)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(address);

        await handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            if (handle.Result.TryGetComponent(out ParticleSystem effectPrefab))
            {
                return Instantiate(effectPrefab, transform);
            }
            else
            {
                Debug.LogError("Error TryGetComponent ParticleSystem effectPrefab.");
                return default;
            }
        }
        else
        {
            Debug.LogError("Error loading via Addressables.");
            return default;
        }
    }

    private ParticleSystem CreateNewEffect()
    {
        return Instantiate(_impactEffectPrefab, transform);
    }

    public void PlayImpactEffect(Vector2 position)
    {
        if (_impactEffectPool.Count > 0)
        {
            var effect = _impactEffectPool.Dequeue();
            EffectSetting(effect, position);
        }
        else
        {
            EffectSetting(CreateNewEffect(), position);
        }
    }

    private async void EffectSetting(ParticleSystem effect, Vector2 position)
    {
        effect.transform.position = position;
        effect.Play();
        await UniTask.WaitForSeconds(effect.main.duration);

        _impactEffectPool.Enqueue(effect);
    }

}
