using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class StabilizationModule : BaseModule
{
    const float TARGET_Z_ROTATION = 0f;

    [SerializeField] float rotationSpeed = 5f;

    Transform _playerTransform;

    [Inject]
    private void Construct(CharacterController player)
    {
        _playerTransform = player.transform;
        transform.parent = _playerTransform;
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        StabilizeRotationAsync().Forget();
    }

    private async UniTaskVoid StabilizeRotationAsync()
    {
        while (true)  
        {
            float currentZRotation = _playerTransform.eulerAngles.z;

            float newZRotation = Mathf.LerpAngle(currentZRotation, TARGET_Z_ROTATION, rotationSpeed * Time.deltaTime);

            _playerTransform.rotation = Quaternion.Euler(0, 0, newZRotation);

            await UniTask.Yield(PlayerLoopTiming.Update);
        }
    }

}
