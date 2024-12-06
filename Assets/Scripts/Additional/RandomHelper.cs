using Unity.Mathematics;

public static class RandomHelper
{
    public static float GetRandomFloat(ref Random random, float min, float max)
    {
        return random.NextFloat(min, max);
    }
}

