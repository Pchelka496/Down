using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct InitialPlacementJob : IJobParallelFor
{
    const float UPDATE_POSITION_Y_DISTANCE_TO_PLAYER = 100f;
    const float UPDATE_POSITION_X_DISTANCE_TO_PLAYER = 50f;

    const float MAX_PLAYER_Y_DISTANCE = 10f;//down
    const float MAX_PLAYER_X_DISTANCE = 20f;//left and right

    const float MAX_X_TRAVEL_DISTANCE_TO_PLAYER = 30f;
    const float MIN_X_TRAVEL_DISTANCE_TO_PLAYER = 10f;

    const float MAX_Y_TRAVEL_DISTANCE_TO_PLAYER = 50f;
    const float MIN_Y_TRAVEL_DISTANCE_TO_PLAYER = 30f;

    // RandomHelper.GetRandomFloat(10f, 100f);

    [ReadOnly] readonly NativeArray<EnumMotionPattern> _positionProcessingMethods;
    [ReadOnly] readonly NativeArray<Vector2> _isolationDistance;

    [WriteOnly] public NativeArray<bool> NeedToChange;

    [ReadOnly] public readonly NativeArray<Vector2> CurrentPosition;
    [WriteOnly] public NativeArray<Vector2> TargetPosition;

    public InitialPlacementJob(ref NativeArray<EnumMotionPattern> positionProcessingMethods,
                               ref NativeArray<Vector2> CurrentPosition,
                               ref NativeArray<Vector2> isolationDistance,
                               ref NativeArray<Vector2> TargetPosition,
                               ref NativeArray<bool> NeedToChange) : this()
    {
        _positionProcessingMethods = positionProcessingMethods;
        this.CurrentPosition = CurrentPosition;
        this.TargetPosition = TargetPosition;
        this.NeedToChange = NeedToChange;
        _isolationDistance = isolationDistance;
    }

    public void Execute(int index)
    {
        if (Mathf.Abs(CharacterPositionMeter.XPosition - CurrentPosition[index].x) > UPDATE_POSITION_X_DISTANCE_TO_PLAYER)
        {
            GlobalXTransfer(index);
            NeedToChange[index] = true;
        }
        if (Mathf.Abs(CharacterPositionMeter.YPosition - CurrentPosition[index].y) > UPDATE_POSITION_Y_DISTANCE_TO_PLAYER)
        {
            GlobalYTransfer(index);
            NeedToChange[index] = true;
        }
    }

    private void GlobalXTransfer(int index)
    {
        var currentPosition = CurrentPosition[index];
        float newXPosition;

        // Проверка типа движения
        if (_positionProcessingMethods[index] == EnumMotionPattern.LinearHorizontalRight ||
            _positionProcessingMethods[index] == EnumMotionPattern.WavyRight ||
            _positionProcessingMethods[index] == EnumMotionPattern.JerkyRight)
        {
            // Если объект движется вправо, перемещаем его слева от игрока на случайное расстояние
            newXPosition = CharacterPositionMeter.XPosition - RandomHelper.GetRandomFloat(MIN_X_TRAVEL_DISTANCE_TO_PLAYER, MAX_X_TRAVEL_DISTANCE_TO_PLAYER);
        }
        else if (_positionProcessingMethods[index] == EnumMotionPattern.LinearHorizontalLeft ||
                 _positionProcessingMethods[index] == EnumMotionPattern.WavyLeft ||
                 _positionProcessingMethods[index] == EnumMotionPattern.JerkyLeft)
        {
            // Если объект движется влево, перемещаем его справа от игрока на случайное расстояние
            newXPosition = CharacterPositionMeter.XPosition + RandomHelper.GetRandomFloat(MIN_X_TRAVEL_DISTANCE_TO_PLAYER, MAX_X_TRAVEL_DISTANCE_TO_PLAYER);
        }
        else
        {
            // Для статичных объектов или вертикальных движений размещаем его по центру игрока по оси X
            newXPosition = CharacterPositionMeter.XPosition + RandomHelper.GetRandomFloat(-MAX_PLAYER_X_DISTANCE, MAX_PLAYER_X_DISTANCE);
        }

        // Обновляем позицию по оси X
        TargetPosition[index] = new(newXPosition, currentPosition.y);

    }

    private void GlobalYTransfer(int index)
    {
        var currentPosition = CurrentPosition[index];
        var newYPosition = CharacterPositionMeter.YPosition - RandomHelper.GetRandomFloat(MIN_Y_TRAVEL_DISTANCE_TO_PLAYER, MAX_Y_TRAVEL_DISTANCE_TO_PLAYER);

        // Обновляем позицию по оси Y
        TargetPosition[index] = new(currentPosition.x, newYPosition);

    }

}
