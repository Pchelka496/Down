using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EffectController
{
    const int INITIAL_POOL_SIZE = 3;
    readonly static string impactEffectPrefabAddress = "Prefab/Effects/ImpactEffect";
    readonly Queue<ParticleSystem> _impactEffectPool = new();
    readonly Transform _transform;

    ParticleSystem _impactEffectPrefab;

    public EffectController(Transform transform)
    {
        _transform = transform;
        Start();
    }

    private async void Start()
    {
        _impactEffectPrefab = await LoadEffectPrefab(impactEffectPrefabAddress);
        _impactEffectPool.Enqueue(_impactEffectPrefab);

        for (int i = 0; i < INITIAL_POOL_SIZE; i++)
        {
            _impactEffectPool.Enqueue(CreateNewEffect());
        }
    }

    private async UniTask<ParticleSystem> LoadEffectPrefab(string address)
    {
        var loadData = await AddressableLouderHelper.LoadAssetAsync<GameObject>(address);

        if (loadData.LoadAsset.TryGetComponent(out ParticleSystem effectPrefab))
        {
            return MonoBehaviour.Instantiate(effectPrefab, _transform);
        }
        else
        {
            Debug.LogError("Error TryGetComponent ParticleSystem effectPrefab.");
            return default;
        }
    }


    private ParticleSystem CreateNewEffect()
    {
        return MonoBehaviour.Instantiate(_impactEffectPrefab, _transform);
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
