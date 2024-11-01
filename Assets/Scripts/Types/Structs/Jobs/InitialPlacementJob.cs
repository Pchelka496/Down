using TMPro;
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
    [WriteOnly] NativeArray<bool> _needToChange;

    [ReadOnly] public readonly NativeArray<Vector2> CurrentPosition;
    [WriteOnly] public NativeArray<Vector2> TargetPosition;

    public InitialPlacementJob(ref NativeArray<EnumMotionPattern> positionProcessingMethods,
                               ref NativeArray<Vector2> CurrentPosition,
                               ref NativeArray<Vector2> TargetPosition,
                               ref NativeArray<bool> NeedToChange) : this()
    {
        _positionProcessingMethods = positionProcessingMethods;
        this.CurrentPosition = CurrentPosition;
        this.TargetPosition = TargetPosition;
        this._needToChange = NeedToChange;
    }

    public void Execute(int index)
    {
        if (Mathf.Abs(CharacterPositionMeter.YPosition - CurrentPosition[index].y) > UPDATE_POSITION_Y_DISTANCE_TO_PLAYER
            ||
            Mathf.Abs(CharacterPositionMeter.XPosition - CurrentPosition[index].x) > UPDATE_POSITION_X_DISTANCE_TO_PLAYER)
        {
            var newXPosition = GlobalXСalculation(index);
            var newYPosition = GlobalYСalculation(index);

            TargetPosition[index] = new(newXPosition, newYPosition);

            _needToChange[index] = true;
        }
    }

    private float GlobalXСalculation(int index)
    {
        var currentPosition = CurrentPosition[index];
        float newXPosition;

        // Проверка типа движения
        if (_positionProcessingMethods[index] == EnumMotionPattern.LinearHorizontalRight ||
            _positionProcessingMethods[index] == EnumMotionPattern.WavyRight ||
            _positionProcessingMethods[index] == EnumMotionPattern.JerkyRight)
        {
            // Если объект движется вправо, перемещаем его слева от игрока на случайное расстояние
            newXPosition = CharacterPositionMeter.XPosition - RandomHelper.GetRandomFloat(MIN_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER, MAX_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER);
        }
        else if (_positionProcessingMethods[index] == EnumMotionPattern.LinearHorizontalLeft ||
                 _positionProcessingMethods[index] == EnumMotionPattern.WavyLeft ||
                 _positionProcessingMethods[index] == EnumMotionPattern.JerkyLeft)
        {
            // Если объект движется влево, перемещаем его справа от игрока на случайное расстояние
            newXPosition = CharacterPositionMeter.XPosition + RandomHelper.GetRandomFloat(MIN_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER, MAX_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER);
        }
        else
        {
            // Для статичных объектов или вертикальных движений размещаем его по центру игрока по оси
            newXPosition = CharacterPositionMeter.XPosition + RandomHelper.GetRandomFloat(-MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER, MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER);
        }

        return newXPosition;
    }

    private float GlobalYСalculation(int index)
    {
        var currentPosition = CurrentPosition[index];
        var newYPosition = CharacterPositionMeter.YPosition - RandomHelper.GetRandomFloat(MIN_Y_TRAVEL_DISTANCE_TO_PLAYER, MAX_Y_TRAVEL_DISTANCE_TO_PLAYER);

        return newYPosition;
    }

}
