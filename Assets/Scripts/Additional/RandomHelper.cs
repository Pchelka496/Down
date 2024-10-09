using System.Threading;

public class RandomHelper
{
    static ThreadLocal<System.Random> _random = new ThreadLocal<System.Random>(() => new System.Random());

    public static float GetRandomFloat(float min, float max)
    {
        return (float)_random.Value.NextDouble() * (max - min) + min;
    }

}
