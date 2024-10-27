using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Zenject;
using System.Threading;

public class StabilizationModule : BaseModule
{
    const float TARGET_Z_ROTATION = 0f;
    const float ANGLE_TOLERANCE = 1f;

    [SerializeField] float _rotationSpeed = 5f;
    [SerializeField] float _maxTorque = 10f;
    [SerializeField] float _dampingFactor = 1f;

    Rigidbody2D _rb;
    CancellationTokenSource _cancellationTokenSource;

    [Inject]
    private void Construct(CharacterController player, LevelManager levelManager)
    {
        _rb = player.Rb;

        transform.parent = player.transform;
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        levelManager.SubscribeToRoundStart(RoundStart);
    }

    private void RoundStart(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundEnd(RoundEnd);

        _cancellationTokenSource = new CancellationTokenSource();
        StabilizeRotationAsync(_cancellationTokenSource.Token).Forget();
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);

        _cancellationTokenSource.Cancel();
    }

    private async UniTaskVoid StabilizeRotationAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var currentZ = _rb.rotation;
            var targetZ = TARGET_Z_ROTATION;

            var angleDifference = Mathf.DeltaAngle(currentZ, targetZ);

            var torque = Mathf.Clamp(angleDifference * _rotationSpeed, -_maxTorque, _maxTorque);

            _rb.AddTorque(torque);

            if (Mathf.Abs(angleDifference) <= ANGLE_TOLERANCE)
            {
                var currentAngularVelocity = _rb.angularVelocity;

                if (Mathf.Abs(currentAngularVelocity) > 0)
                {
                    var dampingTorque = Mathf.Clamp(-currentAngularVelocity * _dampingFactor, -_maxTorque, _maxTorque);
                    _rb.AddTorque(dampingTorque);
                }
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
        }
    }

}


