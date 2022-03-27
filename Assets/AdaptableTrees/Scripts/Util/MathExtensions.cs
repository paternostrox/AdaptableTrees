using System.Collections;
using UnityEngine;

public static class MathExtensions
{
    public static double NextDoubleRange(this System.Random random, double minNumber, double maxNumber)
    {
        return random.NextDouble() * (maxNumber - minNumber) + minNumber;
    }

    public static float NextFloatRange(this System.Random random, float minNumber, float maxNumber)
    {
        return ((float) random.NextDouble()) * (maxNumber - minNumber) + minNumber;
    }
}