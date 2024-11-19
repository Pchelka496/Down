using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Meteorite : MonoBehaviour
{
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] int _damage = 10;

    public void Launch(Vector2 startPosition, float launchForce, Vector2 targetPosition, float angularVelocity = 0f)
    {
        _rb.angularVelocity = angularVelocity;

        transform.position = startPosition;

        Vector2 direction = (targetPosition - startPosition).normalized;

        _rb.velocity = direction * launchForce;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<HealthModule>(out var healthModule))
        {
            healthModule.TestDealDamage(_damage);
        }
    }

    private void Reset()
    {
        _rb = gameObject.GetComponent<Rigidbody2D>();
    }

}