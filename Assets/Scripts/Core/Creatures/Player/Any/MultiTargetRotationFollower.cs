using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Creatures.Player.Any
{
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

        private void EnableRotation()
        {
            if (_rotationObjects.Count == 0) return;

            if (_cts == null)
            {
                _cts = new CancellationTokenSource();
                RotateTowardsMovementAsync(_cts.Token).Forget();
            }
        }

        private void DisableRotation()
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
                var velocity = _targetRb.velocity;

                if (velocity.sqrMagnitude > 0.01f)
                {
                    var targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

                    for (int i = _rotationObjects.Count - 1; i >= 0; i--)
                    {
                        var rotationObject = _rotationObjects[i];
                        try
                        {
                            var currentRotation = rotationObject.TargetTransform.eulerAngles.z;

                            if (Mathf.Abs(Mathf.DeltaAngle(currentRotation,
                                    targetAngle + rotationObject.InitialRotation)) > ROTATION_THRESHOLD)
                            {
                                rotationObject.TargetTransform.rotation =
                                    Quaternion.Euler(0, 0, targetAngle + rotationObject.InitialRotation);
                            }
                        }
                        catch (MissingReferenceException)
                        {
                            UnregisterRotationObject(rotationObject.TargetTransform);
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
}