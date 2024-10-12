using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct MechanicalCounterJob : IJob
{
    const float MinSpeed = 20f;
    const float MaxSpeed = 50f;
    const float SpeedAdjustmentFactor = 0.3f;

    [ReadOnly] readonly NativeArray<float> _targetYPositions;
    [ReadOnly] readonly NativeArray<float> _textSpeeds;
    NativeArray<float> _textYPositions;
    NativeArray<int> _currentDigits;
    NativeArray<int> _targetDigits;

    public float DeltaTime;
    public int Value;

    public MechanicalCounterJob(ref NativeArray<float> targetYPositions,
                                ref NativeArray<float> textYPositions,
                                ref NativeArray<float> textSpeeds,
                                ref NativeArray<int> currentDigits,
                                 ref NativeArray<int> targetDigits

        ) : this()
    {
        _targetYPositions = targetYPositions;
        _targetDigits = targetDigits;
        _textYPositions = textYPositions;
        _textSpeeds = textSpeeds;
        _currentDigits = currentDigits;
    }

    public void Execute()
    {
        int drumCount = _targetDigits.Length;
        SetTargetDigits();

        for (int i = 0; i < drumCount; i++)
        {
            if (_currentDigits[i] != _targetDigits[i])
            {
                if (math.abs(_textYPositions[i] - _targetYPositions[_currentDigits[i]]) < 0.1f)
                {
                    IncrementOrDecrementDigit(i, _targetDigits[i]);
                }
            }

            _textYPositions[i] = _targetYPositions[_currentDigits[i]];

        }
    }

    private void SetTargetDigits()
    {
        for (int i = 0; i < _targetDigits.Length; i++)
        {
            _targetDigits[i] = 0;
        }

        int tempValue = Value;

        for (int i = _targetDigits.Length - 1; i >= 0 && tempValue != 0; i--)
        {
            _targetDigits[i] = math.abs(tempValue % 10);
            tempValue /= 10;
        }
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

}
