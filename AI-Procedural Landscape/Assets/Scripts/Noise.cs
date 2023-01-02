using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public static class Noise {

    public enum NormalizeMode { Local, Global};

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, int seed, int octaves, float persistence, float lacunarity, Vector2 offset, NormalizeMode normalizeMode) {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;

            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }


        if(scale <= 0){
            scale = 0.0001f;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {

                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency ;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency ; 

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }


                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minLocalNoiseHeight){
                    minLocalNoiseHeight = noiseHeight;
                }


                noiseMap[x, y] = noiseHeight;
            }
        }
        for (int y = 0; y < mapHeight; y++){
            for (int x = 0; x < mapWidth; x++){
                /* InverseLerp returns a value between 0 and 1 -> if it's == to min Noise Height then it will return 0
                    if equal to max Noise Height, it returns 1
                */
                if (normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
                else {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2 * maxPossibleHeight / 1.5f);
                    noiseMap[x, y] = Math.Clamp(normalizedHeight, 0, int.MaxValue);
                }


            }
        }

    return noiseMap;
    }

    public static Texture2D QuickNoiseMap(int width, int height, float scale)
    {
        Texture2D noiseMapTexture = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                float noiseValue = Mathf.PerlinNoise((float)x / width * scale, (float)y / height * scale);

                noiseMapTexture.SetPixel(x, y, new Color(0, noiseValue, 0));
            }
        }

        noiseMapTexture.Apply();

        return noiseMapTexture;
    }
}