using UnityEngine;
using Zenject;

public class EnemyCore : MonoBehaviour
{
    const float PUSH_DISTANCE = 10f;

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

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == EnemyManager.ENEMY_LAYER_INDEX)
        {
            var attachedRigidbody = collision?.attachedRigidbody;

            if (attachedRigidbody == null) return;

            var otherTransform = attachedRigidbody.transform;

            Vector3 pushDirection = (otherTransform.position - transform.position).normalized;
            otherTransform.position += pushDirection * PUSH_DISTANCE;
        }
    }

    private void OnDestroy()
    {

    }

}
