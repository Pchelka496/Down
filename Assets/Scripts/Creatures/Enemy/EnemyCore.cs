using UnityEngine;
using Zenject;

public class EnemyCore : MonoBehaviour
{
    static HealthModule _playerHealthModule;

    EnemyVisualPart _visualPart;

    [Inject]
    private void Construct(CharacterController player)
    {
        _playerHealthModule = player.HealthModule;
    }

    public void Initialize(EnemyVisualPart visualPart)
    {
        if (_visualPart != null)
        {
            Destroy(_visualPart);
        }
        if (this == null || gameObject == null)
        {
            if (visualPart != null)
            {
                DestroyImmediate(visualPart);
            }
            return;
        }
        if (visualPart == null) return;

        _visualPart = visualPart;
        _visualPart.transform.parent = transform;
        _visualPart.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private void OnDestroy()
    {

    }

}
