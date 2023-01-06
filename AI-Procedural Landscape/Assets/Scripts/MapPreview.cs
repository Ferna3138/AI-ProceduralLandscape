using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPreview : MonoBehaviour {
    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshData meshData;

    public enum DrawMode { NoiseMap, Mesh, FalloffMap };

    public DrawMode drawMode;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureData;

    public Material terrainMaterial;



    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int editPreviewLOD;

    //Allow for update in preview mode
    public bool autoUpdate;


    public void DrawTexture(Texture2D texture) {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;

        textureRender.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData) {
        this.meshData = meshData;
        meshFilter.sharedMesh = meshData.CreateMesh();

        textureRender.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }


    public void DrawMapInEditor() {
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        //Display method
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numberVertexPerLine,
            meshSettings.numberVertexPerLine,
            heightMapSettings,
            Vector2.zero);


        if (drawMode == DrawMode.NoiseMap) {
            DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
        }

        else if (drawMode == DrawMode.Mesh) {
            DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editPreviewLOD));

            //verticesList = display.meshData.vertices;
        }
        else if (drawMode == DrawMode.FalloffMap) {
            DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numberVertexPerLine),0,1)));
            //verticesList = display.meshData.vertices;
        }

    }

    void OnValuesUpdated() {
        if (!Application.isPlaying) {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated() {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    void OnValidate() {
        //Check if the terrain data and noise data are already suscribed to the On Values Updated List
        if (meshSettings != null) {
            //This line prevents from continously suscribing to the list
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (heightMapSettings != null) {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (textureData != null) {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

}
