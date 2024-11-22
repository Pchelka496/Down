using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct InitialPlacementJob : IJobParallelFor
{
    const float UPDATE_POSITION_Y_DISTANCE_TO_PLAYER = 250f;
    const float UPDATE_POSITION_X_DISTANCE_TO_PLAYER = 60f;

    const float MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER = 15f;
    const float MAX_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER = 50f;
    const float MIN_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER = 0f;

    const float MAX_Y_TRAVEL_DISTANCE_TO_PLAYER = 300f;
    const float MIN_Y_TRAVEL_DISTANCE_TO_PLAYER = 120f;

    [ReadOnly] readonly NativeArray<EnumMotionPattern> _positionProcessingMethods;
    [WriteOnly] NativeArray<bool> _needToChange;

    [ReadOnly] public NativeArray<Vector2> CurrentPosition;
    public NativeArray<Vector2> TargetPosition;

    public float PlayerYPosition;
    public float PlayerXPosition;

    NativeArray<Unity.Mathematics.Random> Randoms;

    public InitialPlacementJob(ref NativeArray<EnumMotionPattern> positionProcessingMethods,
                               ref NativeArray<Vector2> currentPosition,
                               ref NativeArray<Vector2> targetPosition,
                               ref NativeArray<bool> needToChange,
                               ref NativeArray<Unity.Mathematics.Random> randoms
                               ) : this()
    {
        _positionProcessingMethods = positionProcessingMethods;
        CurrentPosition = currentPosition;
        TargetPosition = targetPosition;
        _needToChange = needToChange;
        Randoms = randoms;
    }

    public void Execute(int index)
    {
        if (math.abs(PlayerYPosition - CurrentPosition[index].y) > UPDATE_POSITION_Y_DISTANCE_TO_PLAYER
            || math.abs(PlayerXPosition - CurrentPosition[index].x) > UPDATE_POSITION_X_DISTANCE_TO_PLAYER)
        {
            var random = Randoms[index];
            var newXPosition = GlobalXCalculation(index, ref random);
            var newYPosition = GlobalYCalculation(ref random);

            TargetPosition[index] = new Vector2(newXPosition, newYPosition);
            _needToChange[index] = true;

            Randoms[index] = random;
        }
    }

    private readonly float GlobalXCalculation(int index, ref Unity.Mathematics.Random random)
    {
        if (_positionProcessingMethods[index] == EnumMotionPattern.LinearHorizontalRight ||
            _positionProcessingMethods[index] == EnumMotionPattern.WavyRight ||
            _positionProcessingMethods[index] == EnumMotionPattern.JerkyRight)
        {
            return PlayerXPosition - random.NextFloat(MIN_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER, MAX_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER);
        }
        else if (_positionProcessingMethods[index] == EnumMotionPattern.LinearHorizontalLeft ||
                 _positionProcessingMethods[index] == EnumMotionPattern.WavyLeft ||
                 _positionProcessingMethods[index] == EnumMotionPattern.JerkyLeft)
        {
            return PlayerXPosition + random.NextFloat(MIN_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER, MAX_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER);
        }
        else
        {
            return PlayerXPosition + random.NextFloat(-MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER, MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER);
        }
    }

    private readonly float GlobalYCalculation(ref Unity.Mathematics.Random random)
    {
        return PlayerYPosition - random.NextFloat(MIN_Y_TRAVEL_DISTANCE_TO_PLAYER, MAX_Y_TRAVEL_DISTANCE_TO_PLAYER);
    }

}

