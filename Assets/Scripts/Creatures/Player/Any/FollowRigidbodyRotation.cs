using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class FollowRigidbodyRotation : MonoBehaviour
{
    [SerializeField] Rigidbody2D _targetRb; 
    [SerializeField] float _initialRotation = 0f;   
    private CancellationTokenSource _cts;

    private void Start()
    {
        transform.rotation = Quaternion.Euler(0, 0, _initialRotation);
        EnableRotation();
    }

    public void EnableRotation()
    {
        if (_cts == null)
        {
            _cts = new CancellationTokenSource();
            RotateTowardsMovementAsync(_cts.Token).Forget();
        }
    }

    public void DisableRotation()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private async UniTask RotateTowardsMovementAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            Vector2 velocity = _targetRb.velocity;

            if (velocity.sqrMagnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, targetAngle);
            }

            await UniTask.Yield(token);
        }
    }

    private void OnDisable()
    {
        DisableRotation();
    }

    private void OnDestroy()
    {
        DisableRotation();
    }

}
