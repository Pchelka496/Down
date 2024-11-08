using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MultiTargetRotationFollower : MonoBehaviour
{
    const float ROTATION_THRESHOLD = 0.01f;
    [SerializeField] Rigidbody2D _targetRb;
    readonly List<RotationObject> _rotationObjects = new();
    CancellationTokenSource _cts;

    private void Start()
    {
        EnableRotation();
    }

    public void EnableRotation()
    {
        if (_rotationObjects.Count == 0) return;

        if (_cts == null)
        {
            _cts = new CancellationTokenSource();
            RotateTowardsMovementAsync(_cts.Token).Forget();
        }
    }

    public void DisableRotation()
    {
        ClearToken(ref _cts);
    }

    public void RegisterRotationObject(Transform targetTransform, float initialRotation = 0f)
    {
        _rotationObjects.Add(new RotationObject(targetTransform, initialRotation));

        targetTransform.rotation = Quaternion.Euler(0, 0, initialRotation);

        if (_cts == null) EnableRotation();
    }

    public void UnregisterRotationObject(Transform targetTransform)
    {
        _rotationObjects.RemoveAll(obj => obj.TargetTransform == targetTransform);

        if (_rotationObjects.Count == 0)
        {
            DisableRotation();
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

                foreach (var rotationObject in _rotationObjects)
                {
                    float currentRotation = rotationObject.TargetTransform.eulerAngles.z;

                    if (Mathf.Abs(Mathf.DeltaAngle(currentRotation, targetAngle + rotationObject.InitialRotation)) > ROTATION_THRESHOLD)
                    {
                        rotationObject.TargetTransform.rotation = Quaternion.Euler(0, 0, targetAngle + rotationObject.InitialRotation);
                    }
                }
            }

            await UniTask.Yield(token);
        }
    }

    private void ClearToken(ref CancellationTokenSource cts)
    {
        if (cts == null) return;

        if (!cts.IsCancellationRequested)
        {
            cts.Cancel();
        }

        cts.Dispose();
        cts = null;
    }

    private void OnEnable()
    {
        EnableRotation();
    }

    private void OnDisable()
    {
        DisableRotation();
    }

    private void OnDestroy()
    {
        DisableRotation();
    }

    [System.Serializable]
    public readonly struct RotationObject
    {
        public readonly Transform TargetTransform;
        public readonly float InitialRotation;

        public RotationObject(Transform targetTransform, float initialRotation)
        {
            TargetTransform = targetTransform;
            InitialRotation = initialRotation;
        }
    }

}



