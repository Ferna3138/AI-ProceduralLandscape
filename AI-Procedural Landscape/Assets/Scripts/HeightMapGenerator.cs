using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator {
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCenter) {
        float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseLayers[0], sampleCenter);

        for (int i = 1; i < settings.noiseLayers.Length; i++) {
            float[,] tempValues = Noise.GenerateNoiseMap(width, height, settings.noiseLayers[i], sampleCenter);

            
            
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {

                    //values[x, y] = Mathf.Lerp(values[x,y], tempValues[x,y], 0.5f);

                    //values[x, y] = values[x, y] + tempValues[x, y];
                    values[x, y] = Mathf.Lerp(values[x, y], tempValues[x, y], settings.noiseLayers[i].blend);
                }
            }
            
        }

        

        AnimationCurve heightCurve_safe = new AnimationCurve(settings.heightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                values[i, j] *= heightCurve_safe.Evaluate(values[i, j]) * settings.heightMultiplier;

                if (values[i, j] > maxValue)
                    maxValue = values[i, j];

                if (values[i, j] < minValue)
                    minValue = values[i, j];
            
            }
        }
        return new HeightMap(values, minValue, maxValue);

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
