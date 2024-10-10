using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

//[BurstCompile]
public struct MechanicalCounterJob : IJob
{
    const float MinSpeed = 1f;                                   // Минимальная скорость анимации
    const float MaxSpeed = 10f;                                  // Максимальная скорость анимации
    const float SpeedAdjustmentFactor = 0.5f;                    // Фактор корректировки скорости
    const float Epsilon = 0.1f;                                  // Порог для определения близости к значению

    [ReadOnly] public NativeArray<float> _targetYPositions;      // Целевая позиция Y для каждого числа

    NativeArray<float> _textYPositions;                          // Текущие позиции текста
    NativeArray<float> _textSpeeds;                              // Текущие скорости для каждого барабана
    NativeArray<bool> _useSpareDrum;                             // Указывает, используется ли запасной барабан для анимации

    public float DeltaTime;                                      // Время кадра для расчёта анимации
    public int Value;                                            // Текущее значение на барабанах (например, 89891)

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
        // Получаем цифры из числа Value для каждого барабана
        int drumCount = _textYPositions.Length;  // Количество барабанов
        int[] digits = new int[drumCount];

        // Преобразуем значение в массив цифр, начиная с конца, добавляем нули в начало
        for (int i = drumCount - 1; i >= 0; i--)
        {
            // Получаем каждую цифру из Value, если число закончилось, заполняем нулями
            digits[i] = Value > 0 ? Value % 10 : 0;
            Value /= 10;
        }

        // Для каждого барабана
        for (int i = 0; i < drumCount; i++)
        {
            int currentDigit = digits[i];

            // Преобразуем значение в индекс с учётом смещения
            int currentValueIndex = GetIndexForDigit(currentDigit);

            // Определяем использование запасного барабана для перехода
            if (currentDigit == 9)
            {
                _useSpareDrum[i] = true;
            }

            var targetY = _targetYPositions[currentValueIndex];  // Целевая позиция для текущей цифры

            float currentY = _textYPositions[i];

            // Проверка, если барабан близок к позиции индекса 10 (цифра 9)
            if (math.abs(currentY - _targetYPositions[10]) < Epsilon)
            {
                _textYPositions[i] = _targetYPositions[0];  // Моментально перемещаем на позицию индекса 0
                _textSpeeds[i] = 0f;  // Обнуляем скорость, чтобы не было дальнейшей анимации
                continue;  // Переходим к следующему барабану
            }

            // Расчёт разницы между текущей и целевой позицией
            float distanceToTarget = targetY - currentY;

            // Корректировка скорости в зависимости от расстояния до цели
            if (math.abs(distanceToTarget) > 0.01f)  // Допустимый порог для остановки
            {
                // Если барабан отстаёт, увеличиваем скорость, если опережает — уменьшаем
                _textSpeeds[i] += distanceToTarget * SpeedAdjustmentFactor * DeltaTime;

                // Ограничиваем скорость между минимальной и максимальной
                _textSpeeds[i] = math.clamp(_textSpeeds[i], MinSpeed, MaxSpeed);

                // Обновление позиции текста с использованием lerp, кроме перехода с 10 на 0
                _textYPositions[i] = math.lerp(currentY, targetY, _textSpeeds[i] * DeltaTime);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndexForDigit(int digit)
    {
        if (digit == 9)
        {
            return 0;  // Цифра 9 соответствует индексу 0
        }
        else
        {
            return digit + 1;  // Остальные цифры смещены на +1
        }
    }
}
