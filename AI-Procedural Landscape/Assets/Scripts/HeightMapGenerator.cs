using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator {
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCenter) {

        float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseLayers[0], sampleCenter);

        List<float[,]> noiseMaps = new List<float[,]>();

        
        for (int i = 1; i < settings.noiseLayers.Length; i++) {
            if(settings.noiseLayers[i].enable)
                noiseMaps.Add(Noise.GenerateNoiseMap(width, height, settings.noiseLayers[i], sampleCenter));
        }

        AnimationCurve heightCurve_safe = new AnimationCurve(settings.heightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (noiseMaps.Count > 0) {
                    int index = 1;
                    foreach (float[,] tempNoise in noiseMaps) {
                    
                        
                        //values[i, j] = Mathf.Max(0.5f - Mathf.Abs(values[i, j] - tempNoise[i, j]), 0.0f) / 0.5f;
                        switch (settings.blendType) {
                            case (BlendType.polynomial):
                                if(index == 1)
                                    values[i, j] = PolySmoothMin(values[i, j], tempNoise[i, j], 0.5f);
                                else
                                    values[i, j] = average(values[i, j], tempNoise[i, j]);
                                break;
                            case (BlendType.average):
                                values[i, j] = sminCubic(values[i, j], tempNoise[i, j],0.5f);
                                break;
                        }

                        values[i, j] *= heightCurve_safe.Evaluate(values[i, j]) * settings.heightMultiplier;

                        if (values[i, j] > maxValue)
                            maxValue = values[i, j];

                        if (values[i, j] < minValue)
                            minValue = values[i, j];

                        index++;
                    }
                }
                else {
                    values[i, j] *= heightCurve_safe.Evaluate(values[i, j]) * settings.heightMultiplier;

                    if (values[i, j] > maxValue)
                        maxValue = values[i, j];

                    if (values[i, j] < minValue)
                        minValue = values[i, j];
                }
                
            }
        }



        return new HeightMap(values, minValue, maxValue);

    }

    //Blending Modes
    static float PolySmoothMin(float a, float b, float bias) {
        float h = Mathf.Clamp(0.5f + 0.5f * (b - a) / bias, 0.0f, 1.0f);
        return Mathf.Min(b, a, h) - bias * h * (1.0f - h);
    }

    static float average(float a, float b) {
        return ((a + b) / 2)/20.0f;
    }

    static float sminCubic(float a, float b, float k) {
        float h = Mathf.Max(k - Mathf.Abs(a - b), 0.0f) / k;
        return Mathf.Min(a, b) - h * h * h * k * (1.0f / 6.0f);
    }
}

public struct HeightMap {
    public readonly float[,] values;
    public readonly float minValue;
    public readonly float maxValue;

    public HeightMap(float[,] values, float minValue, float maxValue) {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }

}
