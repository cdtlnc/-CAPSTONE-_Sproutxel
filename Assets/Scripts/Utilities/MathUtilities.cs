using System;
using UnityEngine;

namespace Sproutxel.MathUtilities
{
    public class BellCurve
    {
        public static float GetFactor(float value, float optimal, float spread)
        {
            float currentGaussian = Mathf.Exp(-Mathf.Pow(value - optimal, 2f) / (2f * spread * spread));
            float gaussianBoundary = Mathf.Exp(-Mathf.Pow(50f - optimal, 2f) / (2f * spread * spread));

            return (currentGaussian -  gaussianBoundary) / (1f - gaussianBoundary);
        }
    }
}
