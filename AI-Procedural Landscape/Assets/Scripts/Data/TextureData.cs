using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu()]
public class TextureData : UpdatableData {
    const int textureSize = 512;
    const TextureFormat textureFormat = TextureFormat.RGB565;

    
    public Biome[] biomes;
    public Layer[] layers;
    
    float savedMinHeight;
    float savedMaxHeight;

    float savedNorth;
    float savedEast;

    public void ApplyToMaterial(Material material) {
        //layers = biomes;

        for (int i = 0; i < biomes.Length; i++) {
            //material.SetBuffer(biomes[i].name, );
        }

        material.SetInt("biomeCount", biomes.Length);
        

        material.SetInt("layerCount", layers.Length);
        material.SetColorArray("baseColours", layers.Select(x => x.tint).ToArray());
        material.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
        material.SetFloatArray("baseBlends", layers.Select(x => x.blendStrength).ToArray());
        material.SetFloatArray("baseColourStrength", layers.Select(x => x.tintStrength).ToArray());
        material.SetFloatArray("baseTextureScales", layers.Select(x => x.textureScale).ToArray());
        Texture2DArray texturesArray = GenerateTextureArray(layers.Select(x => x.texture).ToArray());
        material.SetTexture("baseTextures", texturesArray);
        

        UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
    }

    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight) {
        savedMaxHeight = maxHeight;
        savedMinHeight = minHeight;

        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
    }

    public void UpdateBlendBias(Material material, Vector2 position) {
        savedNorth = position.x;
        savedEast = position.y;

        Biome tempBiome;
        //North Biome
        if (position.y > 0) {
            if(biomes.Length > 0)
                tempBiome = biomes[1];
            else
                tempBiome = biomes[0];
        }
        else { // South Biome
            if (biomes.Length > 1)
                tempBiome = biomes[2];
            else
                tempBiome = biomes[0];
        }

        
        Layer[] tempLayers = tempBiome.biomeLayers;
        
        material.SetColorArray("blendBaseColours", tempLayers.Select(x => x.tint).ToArray());
        material.SetFloatArray("blendBaseStartHeights", tempLayers.Select(x => x.startHeight).ToArray());
        material.SetFloatArray("blendBaseBlends", tempLayers.Select(x => x.blendStrength).ToArray());
        material.SetFloatArray("blendBaseColourStrength", tempLayers.Select(x => x.tintStrength).ToArray());
        material.SetFloatArray("blendBaseTextureScales", tempLayers.Select(x => x.textureScale).ToArray());

        float blendBias = ((Mathf.Sin(savedNorth) + 1) / 2);

        Debug.Log(tempBiome.name + " - " + blendBias);

        material.SetFloat("blendBias", blendBias);
        
    }

    Texture2DArray GenerateBiomesTex(Biome[] biomes) {
        int layersLength = biomes.Select(x => x.biomeLayers).ToArray().Length;

        Texture2DArray textureArray = new Texture2DArray(textureSize, textureSize, biomes.Length * layersLength, textureFormat, true);
        for (int i = 0; i < biomes.Length; i++) {
            for (int j = 0; j < layers.Length; j++) {
                textureArray.SetPixels(layers[j].texture.GetPixels(), j);
            }
        }
        textureArray.Apply();
        return textureArray;
    }

    Texture2DArray GenerateTextureArray(Texture2D[] textures) {
        Texture2DArray textureArray = new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);
        for (int i = 0; i < textures.Length; i++) {
            textureArray.SetPixels(textures[i].GetPixels(), i);
        }
        textureArray.Apply();
        return textureArray;
    }

    float CalculateBias(Vector2 position) {
        return (Mathf.Sin(0.5f * position.y) + 1) / 2;
    }

    [System.Serializable]
    public class Biome {
        public string name;
        public Layer[] biomeLayers;
        public Color biomeTint;
        public Gradient biomeGradient;

        [Range(0, 1)]
        public float biomeTintStrength;
        [Range(0, 1)]
        public float biomeStartHeight;

        void Awake() {
            foreach (Layer layer in biomeLayers) {
                layer.name = name;
            }
        }
    }

    [System.Serializable]
    public class Layer {
        public string name;
        public Texture2D texture;
        public Color tint;
        [Range(0,1)]
        public float tintStrength;
        [Range(0, 1)]
        public float startHeight;
        [Range(0, 1)]
        public float blendStrength;
        public float textureScale;
    }

    
}
