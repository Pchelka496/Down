using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct InitialPlacementJob : IJobParallelFor
{
    const float UPDATE_POSITION_Y_DISTANCE_TO_PLAYER = 120f;
    const float UPDATE_POSITION_X_DISTANCE_TO_PLAYER = 50f;

    const float MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER = 10f;

    const float MAX_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER = 40f;
    const float MIN_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER = 10f;

    const float MAX_Y_TRAVEL_DISTANCE_TO_PLAYER = 110f;
    const float MIN_Y_TRAVEL_DISTANCE_TO_PLAYER = 50f;

    // RandomHelper.GetRandomFloat(10f, 100f);

    [ReadOnly] readonly NativeArray<EnumMotionPattern> _positionProcessingMethods;
    [ReadOnly] readonly NativeArray<Vector2> _isolationDistance;
    
    [WriteOnly] NativeArray<bool> _needToChange;

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
        this._needToChange = NeedToChange;
        _isolationDistance = isolationDistance;
    }

    public void Execute(int index)
    {
        if (Mathf.Abs(CharacterPositionMeter.XPosition - CurrentPosition[index].x) > UPDATE_POSITION_X_DISTANCE_TO_PLAYER)
        {
            GlobalXTransfer(index);
            _needToChange[index] = true;
        }
        if (Mathf.Abs(CharacterPositionMeter.YPosition - CurrentPosition[index].y) > UPDATE_POSITION_Y_DISTANCE_TO_PLAYER)
        {
            GlobalYTransfer(index);
            _needToChange[index] = true;
        }
    }

    private void GlobalXTransfer(int index)
    {
        var currentPosition = CurrentPosition[index];
        float newXPosition;

        // �������� ���� ��������
        if (_positionProcessingMethods[index] == EnumMotionPattern.LinearHorizontalRight ||
            _positionProcessingMethods[index] == EnumMotionPattern.WavyRight ||
            _positionProcessingMethods[index] == EnumMotionPattern.JerkyRight)
        {
            // ���� ������ �������� ������, ���������� ��� ����� �� ������ �� ��������� ����������
            newXPosition = CharacterPositionMeter.XPosition - RandomHelper.GetRandomFloat(MIN_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER, MAX_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER);
        }
        else if (_positionProcessingMethods[index] == EnumMotionPattern.LinearHorizontalLeft ||
                 _positionProcessingMethods[index] == EnumMotionPattern.WavyLeft ||
                 _positionProcessingMethods[index] == EnumMotionPattern.JerkyLeft)
        {
            // ���� ������ �������� �����, ���������� ��� ������ �� ������ �� ��������� ����������
            newXPosition = CharacterPositionMeter.XPosition + RandomHelper.GetRandomFloat(MIN_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER, MAX_X_MOVEBLE_ENEMY_TRAVEL_DISTANCE_TO_PLAYER);
        }
        else
        {
            // ��� ��������� �������� ��� ������������ �������� ��������� ��� �� ������ ������ �� ��� X
            newXPosition = CharacterPositionMeter.XPosition + RandomHelper.GetRandomFloat(-MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER, MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER);
        }

        // ��������� ������� �� ��� X
        TargetPosition[index] = new(newXPosition, currentPosition.y);

    }

    private void GlobalYTransfer(int index)
    {
        var currentPosition = CurrentPosition[index];
        var newYPosition = CharacterPositionMeter.YPosition - RandomHelper.GetRandomFloat(MIN_Y_TRAVEL_DISTANCE_TO_PLAYER, MAX_Y_TRAVEL_DISTANCE_TO_PLAYER);

        TargetPosition[index] = new(currentPosition.x, newYPosition);

    }

}
