using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct InitialPlacementJob : IJobParallelFor
{
    const float UPDATE_POSITION_Y_DISTANCE_TO_PLAYER = 250f;
    const float UPDATE_POSITION_X_DISTANCE_TO_PLAYER = 60f;

    const float MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER = 15f;
    const float MAX_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER = 50f;
    const float MIN_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER = 0f;

    const float MAX_Y_TRAVEL_DISTANCE_TO_PLAYER = 200f;
    const float MIN_Y_TRAVEL_DISTANCE_TO_PLAYER = 80f;

    [ReadOnly] readonly NativeArray<EnumMotionPattern> _positionProcessingMethods;
    [ReadOnly] NativeArray<Vector2> _isolateStaticElementDistance;
    [WriteOnly] NativeArray<bool> _needToChange;

    [ReadOnly] public readonly NativeArray<Vector2> CurrentPosition;
    public NativeArray<Vector2> TargetPosition;

    public InitialPlacementJob(ref NativeArray<EnumMotionPattern> positionProcessingMethods,
                               ref NativeArray<Vector2> currentPosition,
                               ref NativeArray<Vector2> targetPosition,
                               ref NativeArray<Vector2> isolateStaticElementDistance,
                               ref NativeArray<bool> needToChange) : this()
    {
        _positionProcessingMethods = positionProcessingMethods;
        _isolateStaticElementDistance = isolateStaticElementDistance;
        CurrentPosition = currentPosition;
        TargetPosition = targetPosition;
        _needToChange = needToChange;
    }

    public void Execute(int index)
    {
        if (Mathf.Abs(CharacterPositionMeter.YPosition - CurrentPosition[index].y) > UPDATE_POSITION_Y_DISTANCE_TO_PLAYER
            || Mathf.Abs(CharacterPositionMeter.XPosition - CurrentPosition[index].x) > UPDATE_POSITION_X_DISTANCE_TO_PLAYER)
        {
            var newXPosition = GlobalXCalculation(index);
            var newYPosition = GlobalYCalculation(index);

            Vector2 newPosition = new Vector2(newXPosition, newYPosition);

            //for (int i = 0; i < CurrentPosition.Length; i++)
            //{
            //    if (i == index) continue; 

            //    float distance = Vector2.Distance(newPosition, CurrentPosition[i]);
            //    float requiredDistance = Mathf.Max(_isolateStaticElementDistance[index].x, _isolateStaticElementDistance[i].x);

            //    if (distance < requiredDistance)
            //    {
            //        Vector2 direction = (newPosition - CurrentPosition[i]).normalized;
            //        newPosition = CurrentPosition[i] + direction * requiredDistance;
            //    }
            //}

            TargetPosition[index] = newPosition;
            _needToChange[index] = true;
        }
    }

    private float GlobalXCalculation(int index)
    {
        if (_positionProcessingMethods[index] == EnumMotionPattern.LinearHorizontalRight ||
            _positionProcessingMethods[index] == EnumMotionPattern.WavyRight ||
            _positionProcessingMethods[index] == EnumMotionPattern.JerkyRight)
        {
            return CharacterPositionMeter.XPosition - RandomHelper.GetRandomFloat(MIN_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER, MAX_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER);
        }
        else if (_positionProcessingMethods[index] == EnumMotionPattern.LinearHorizontalLeft ||
                 _positionProcessingMethods[index] == EnumMotionPattern.WavyLeft ||
                 _positionProcessingMethods[index] == EnumMotionPattern.JerkyLeft)
        {
            return CharacterPositionMeter.XPosition + RandomHelper.GetRandomFloat(MIN_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER, MAX_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER);
        }
        else
        {
            return CharacterPositionMeter.XPosition + RandomHelper.GetRandomFloat(-MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER, MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER);
        }
    }

    private float GlobalYCalculation(int index)
    {
        return CharacterPositionMeter.YPosition - RandomHelper.GetRandomFloat(MIN_Y_TRAVEL_DISTANCE_TO_PLAYER, MAX_Y_TRAVEL_DISTANCE_TO_PLAYER);
    }

}
