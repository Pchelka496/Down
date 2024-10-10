using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

//[BurstCompile]
public struct MechanicalCounterJob : IJob
{
    const float MinSpeed = 1f;                                   // ����������� �������� ��������
    const float MaxSpeed = 10f;                                  // ������������ �������� ��������
    const float SpeedAdjustmentFactor = 0.5f;                    // ������ ������������� ��������
    const float Epsilon = 0.1f;                                  // ����� ��� ����������� �������� � ��������

    [ReadOnly] public NativeArray<float> _targetYPositions;      // ������� ������� Y ��� ������� �����

    NativeArray<float> _textYPositions;                          // ������� ������� ������
    NativeArray<float> _textSpeeds;                              // ������� �������� ��� ������� ��������
    NativeArray<bool> _useSpareDrum;                             // ���������, ������������ �� �������� ������� ��� ��������

    public float DeltaTime;                                      // ����� ����� ��� ������� ��������
    public int Value;                                            // ������� �������� �� ��������� (��������, 89891)

    public MechanicalCounterJob(ref NativeArray<float> targetYPositions,
                                ref NativeArray<float> textYPositions,
                                ref NativeArray<float> textSpeeds,
                                ref NativeArray<bool> useSpareDrum) : this()
    {
        _targetYPositions = targetYPositions;
        _textYPositions = textYPositions;
        _textSpeeds = textSpeeds;
        _useSpareDrum = useSpareDrum;
    }

    public void Execute()
    {
        // �������� ����� �� ����� Value ��� ������� ��������
        int drumCount = _textYPositions.Length;  // ���������� ���������
        int[] digits = new int[drumCount];

        // ����������� �������� � ������ ����, ������� � �����, ��������� ���� � ������
        for (int i = drumCount - 1; i >= 0; i--)
        {
            // �������� ������ ����� �� Value, ���� ����� �����������, ��������� ������
            digits[i] = Value > 0 ? Value % 10 : 0;
            Value /= 10;
        }

        // ��� ������� ��������
        for (int i = 0; i < drumCount; i++)
        {
            int currentDigit = digits[i];

            // ����������� �������� � ������ � ������ ��������
            int currentValueIndex = GetIndexForDigit(currentDigit);

            // ���������� ������������� ��������� �������� ��� ��������
            if (currentDigit == 9)
            {
                _useSpareDrum[i] = true;
            }

            var targetY = _targetYPositions[currentValueIndex];  // ������� ������� ��� ������� �����

            float currentY = _textYPositions[i];

            // ��������, ���� ������� ������ � ������� ������� 10 (����� 9)
            if (math.abs(currentY - _targetYPositions[10]) < Epsilon)
            {
                _textYPositions[i] = _targetYPositions[0];  // ����������� ���������� �� ������� ������� 0
                _textSpeeds[i] = 0f;  // �������� ��������, ����� �� ���� ���������� ��������
                continue;  // ��������� � ���������� ��������
            }

            // ������ ������� ����� ������� � ������� ��������
            float distanceToTarget = targetY - currentY;

            // ������������� �������� � ����������� �� ���������� �� ����
            if (math.abs(distanceToTarget) > 0.01f)  // ���������� ����� ��� ���������
            {
                // ���� ������� ������, ����������� ��������, ���� ��������� � ���������
                _textSpeeds[i] += distanceToTarget * SpeedAdjustmentFactor * DeltaTime;

                // ������������ �������� ����� ����������� � ������������
                _textSpeeds[i] = math.clamp(_textSpeeds[i], MinSpeed, MaxSpeed);

                // ���������� ������� ������ � �������������� lerp, ����� �������� � 10 �� 0
                _textYPositions[i] = math.lerp(currentY, targetY, _textSpeeds[i] * DeltaTime);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndexForDigit(int digit)
    {
        if (digit == 9)
        {
            return 0;  // ����� 9 ������������� ������� 0
        }
        else
        {
            return digit + 1;  // ��������� ����� ������� �� +1
        }
    }
}
