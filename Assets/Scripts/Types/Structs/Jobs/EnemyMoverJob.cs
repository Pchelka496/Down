using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public struct EnemyMoverJob : IJobParallelForTransform
{
    [ReadOnly] readonly NativeArray<float> _speeds;
    [ReadOnly] readonly NativeArray<EnumMotionPattern> _positionProcessingMethods;
    [ReadOnly] readonly NativeArray<float2> _motionCharacteristic;
    [ReadOnly] readonly NativeArray<Vector2> _initialPosition;
    NativeArray<bool> _needToChangeInitialPosition;

    [ReadOnly] public float Time;
    [ReadOnly] public float DeltaTime;

    [WriteOnly] NativeArray<Vector2> _currentPosition;

    public EnemyMoverJob(ref NativeArray<float> speeds,
                          ref NativeArray<EnumMotionPattern> positionProcessingMethods,
                          ref NativeArray<float2> motionCharacteristic,
                          ref NativeArray<Vector2> currentPosition,
                          ref NativeArray<Vector2> initialPosition,
                          ref NativeArray<bool> needToChangeInitialPosition
                          ) : this()
    {
        _speeds = speeds;
        _positionProcessingMethods = positionProcessingMethods;
        _motionCharacteristic = motionCharacteristic;
        _currentPosition = currentPosition;
        _initialPosition = initialPosition;
        _needToChangeInitialPosition = needToChangeInitialPosition;
    }

    public void Execute(int index, TransformAccess transform)
    {
        if (_needToChangeInitialPosition[index])
        {
            SetInitialPosition(transform, index);
            _needToChangeInitialPosition[index] = false;
        }

        switch (_positionProcessingMethods[index])
        {
            case EnumMotionPattern.LinearHorizontalRight:
                {
                    LinearMoving(transform, index, Vector2.right);
                    break;
                }
            case EnumMotionPattern.LinearHorizontalLeft:
                {
                    LinearMoving(transform, index, Vector2.left);
                    break;
                }
            case EnumMotionPattern.LinearVerticalUp:
                {
                    LinearMoving(transform, index, Vector2.up);
                    break;
                }
            case EnumMotionPattern.LinearVerticalDown:
                {
                    LinearMoving(transform, index, Vector2.down);
                    break;
                }
            case EnumMotionPattern.WavyRight:
                {
                    WaveMovement(transform, index, Vector2.right);
                    break;
                }
            case EnumMotionPattern.WavyLeft:
                {
                    WaveMovement(transform, index, Vector2.left);
                    break;
                }
            case EnumMotionPattern.JerkyRight:
                {
                    JerkyMoving(transform, index, Vector2.right);
                    break;
                }
            case EnumMotionPattern.JerkyLeft:
                {
                    JerkyMoving(transform, index, Vector2.left);
                    break;
                }
            case EnumMotionPattern.Static:
                {
                    return;
                }
            case EnumMotionPattern.TurningOnTheSpot:
                {
                    RotateOnTheSpot(transform, index);
                    return;
                }
            default:
                {
                    return;
                }
        }
    }

    private void SetInitialPosition(TransformAccess transform, int index)
    {
        var newPosition = _initialPosition[index];

        transform.position = newPosition;
        _currentPosition[index] = newPosition;
    }

    private void LinearMoving(TransformAccess transform, int index, Vector2 direction)
    {
        float speed = _speeds[index];
        var newPosition = transform.position + (Vector3)(DeltaTime * speed * direction);

        transform.position = newPosition;
        _currentPosition[index] = newPosition;
    }

    private void WaveMovement(TransformAccess transform, int index, Vector2 direction)
    {
        var speed = _speeds[index];
        var frequency = _motionCharacteristic[index].x;
        var amplitude = _motionCharacteristic[index].y;

        var horizontalMovement = DeltaTime * speed * direction;

        var waveOffsetY = Mathf.Sin(Time * frequency) * amplitude;

        var newPosition = transform.position + new Vector3(horizontalMovement.x, waveOffsetY, 0);

        transform.position = newPosition;
        _currentPosition[index] = newPosition;
    }

    private void JerkyMoving(TransformAccess transform, int index, Vector2 direction)
    {
        //_speeds[index]

    }

    private void RotateOnTheSpot(TransformAccess transform, int index)
    {
        float rotationSpeed = _speeds[index];
        float newRotation = transform.rotation.eulerAngles.z + rotationSpeed * DeltaTime;
        transform.rotation = Quaternion.Euler(0f, 0f, newRotation);
    }

}
