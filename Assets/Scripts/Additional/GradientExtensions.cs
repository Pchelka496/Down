using UnityEngine;

public static class GradientExtensions
{
    public static bool AreEqual(this Gradient gradientA, Gradient gradientB)
    {
        if (gradientA == null || gradientB == null)
            return gradientA == gradientB; 

        if (gradientA.mode != gradientB.mode)
            return false;

        if (!AreColorKeysEqual(gradientA.colorKeys, gradientB.colorKeys))
            return false;

        if (!AreAlphaKeysEqual(gradientA.alphaKeys, gradientB.alphaKeys))
            return false;

        return true;
    }

    private static bool AreColorKeysEqual(GradientColorKey[] keysA, GradientColorKey[] keysB)
    {
        if (keysA.Length != keysB.Length)
            return false;

        for (int i = 0; i < keysA.Length; i++)
        {
            if (keysA[i].color != keysB[i].color || keysA[i].time != keysB[i].time)
                return false;
        }

        return true;
    }

    private static bool AreAlphaKeysEqual(GradientAlphaKey[] keysA, GradientAlphaKey[] keysB)
    {
        if (keysA.Length != keysB.Length)
            return false;

        for (int i = 0; i < keysA.Length; i++)
        {
            if (keysA[i].alpha != keysB[i].alpha || keysA[i].time != keysB[i].time)
                return false;
        }

        return true;
    }

}

