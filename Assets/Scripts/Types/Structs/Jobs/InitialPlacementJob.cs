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
            var newXPosition = GlobalX�alculation(index);
            var newYPosition = GlobalY�alculation(index);

            TargetPosition[index] = new(newXPosition, newYPosition);

            _needToChange[index] = true;
        }
    }

    private float GlobalX�alculation(int index)
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
            // ��� ��������� �������� ��� ������������ �������� ��������� ��� �� ������ ������ �� ���
            newXPosition = CharacterPositionMeter.XPosition + RandomHelper.GetRandomFloat(-MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER, MAX_X_STATIC_ENEMY_DISTANCE_TO_PLAYER);
        }

        return newXPosition;
    }

    private float GlobalY�alculation(int index)
    {
        var currentPosition = CurrentPosition[index];
        var newYPosition = CharacterPositionMeter.YPosition - RandomHelper.GetRandomFloat(MIN_Y_TRAVEL_DISTANCE_TO_PLAYER, MAX_Y_TRAVEL_DISTANCE_TO_PLAYER);

        return newYPosition;
    }

}
