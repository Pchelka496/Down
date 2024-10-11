using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct MechanicalCounterJob : IJob
{
    const float MinSpeed = 20f;
    const float MaxSpeed = 50f;
    const float SpeedAdjustmentFactor = 0.3f;

    [ReadOnly] public readonly NativeArray<float> _targetYPositions;
    NativeArray<float> _textYPositions;
    NativeArray<float> _textSpeeds;
    NativeArray<int> _currentDigits;

    public float DeltaTime;
    public int Value;

    public MechanicalCounterJob(ref NativeArray<float> targetYPositions,
                                ref NativeArray<float> textYPositions,
                                ref NativeArray<float> textSpeeds,
                                ref NativeArray<int> currentDigits) : this()
    {
        _targetYPositions = targetYPositions;
        _textYPositions = textYPositions;
        _textSpeeds = textSpeeds;
        _currentDigits = currentDigits;
    }

    public void Execute()
    {
        int drumCount = _textYPositions.Length;

        int[] targetDigits = GetTargetDigits(drumCount);

        for (int i = 0; i < drumCount; i++)
        {
            if (_currentDigits[i] != targetDigits[i])
            {
                if (math.abs(_textYPositions[i] - _targetYPositions[_currentDigits[i]]) < 0.1f)
                {
                    IncrementOrDecrementDigit(i, targetDigits[i]);
                }
                SmoothTransfer(i, targetDigits[i]);
                //_textYPositions[i] = _targetYPositions[_currentDigits[i]];
            }
        }
    }

    private int[] GetTargetDigits(int drumCount)
    {
        int[] targetDigits = new int[drumCount];
        int tempValue = Value;

        // Преобразуем значение Value в массив цифр, начиная с конца
        for (int i = drumCount - 1; i >= 0; i--)
        {
            targetDigits[i] = tempValue > 0 ? tempValue % 10 : 0;
            tempValue /= 10;
        }

        return targetDigits;
    }

    private void IncrementOrDecrementDigit(int index, int targetDigit)
    {
        int currentDigit = _currentDigits[index];

        int delta = targetDigit - currentDigit;

        int pathUp = (delta + CharacterPositionMeter.DRUM_NUMBER_OF_NUMBERS) % CharacterPositionMeter.DRUM_NUMBER_OF_NUMBERS;
        int pathDown = (currentDigit - targetDigit + CharacterPositionMeter.DRUM_NUMBER_OF_NUMBERS) % CharacterPositionMeter.DRUM_NUMBER_OF_NUMBERS;

        if (pathUp <= pathDown)
        {
            _currentDigits[index] = (currentDigit + 1) % CharacterPositionMeter.DRUM_NUMBER_OF_NUMBERS;
        }
        else
        {
            _currentDigits[index] = (currentDigit - 1 + CharacterPositionMeter.DRUM_NUMBER_OF_NUMBERS) % CharacterPositionMeter.DRUM_NUMBER_OF_NUMBERS;
        }
    }

    private void SmoothTransfer(int drumIndex, int targetIndex)
    {
        //_textYPositions[drumIndex] = _targetYPositions[targetIndex];

        var oldPosition = _textYPositions[drumIndex];
        var targetPosition = _targetYPositions[targetIndex];

        _textYPositions[drumIndex] = math.lerp(oldPosition, targetPosition, 0.1f);
    }

}

