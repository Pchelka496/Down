using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Creatures.Player.Any
{
    public class MultiTargetGlobalRotationSetter : MonoBehaviour
    {
        [SerializeField] RotationObject[] _initialRotationObjects;
        readonly List<RotationObject> _rotationObjects = new();
        CancellationTokenSource _cts;

        private void Start()
        {
            foreach (var rotationObject in _initialRotationObjects)
            {
                if (rotationObject.TargetTransform != null)
                {
                    _rotationObjects.Add(rotationObject);
                }
            }

            StartRotationUpdate();
        }

        public void RegisterRotationObject(Transform targetTransform, float initialGlobalRotation)
        {
            _rotationObjects.Add(new RotationObject(targetTransform, initialGlobalRotation));

            if (_cts == null)
            {
                StartRotationUpdate();
            }
        }

        public void UnregisterRotationObject(Transform targetTransform)
        {
            _rotationObjects.RemoveAll(obj => obj.TargetTransform == targetTransform);

            if (_rotationObjects.Count == 0)
            {
                StopRotationUpdate();
            }
        }

        private void StartRotationUpdate()
        {
            if (_cts == null)
            {
                _cts = new CancellationTokenSource();
                UpdateGlobalRotationsAsync(_cts.Token).Forget();
            }
        }

        private void StopRotationUpdate()
        {
            ClearToken(ref _cts);
        }

        private async UniTask UpdateGlobalRotationsAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                foreach (var rotationObject in _rotationObjects)
                {
                    if (rotationObject.TargetTransform != null)
                    {
                        var currentRotation = rotationObject.TargetTransform.rotation.eulerAngles.z;

                        if (Mathf.Abs(currentRotation - rotationObject.InitialGlobalRotation) > 0.01f)
                        {
                            rotationObject.TargetTransform.rotation =
                                Quaternion.Euler(0, 0, rotationObject.InitialGlobalRotation);
                        }
                    }
                }

                await UniTask.Yield(token);
            }
        }

        private void OnEnable()
        {
            if (_rotationObjects.Count > 0) StartRotationUpdate();
        }

        private void OnDisable()
        {
            StopRotationUpdate();
        }

        private void OnDestroy()
        {
            StopRotationUpdate();
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

        [System.Serializable]
        public struct RotationObject
        {
            public Transform TargetTransform;
            public float InitialGlobalRotation;

            public RotationObject(Transform targetTransform, float initialGlobalRotation)
            {
                TargetTransform = targetTransform;
                InitialGlobalRotation = initialGlobalRotation;
            }
        }
    }
}