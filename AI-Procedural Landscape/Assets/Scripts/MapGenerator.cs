using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour {
    public enum DrawMode {NoiseMap, ColourMap, Mesh, FalloffMap};

    public bool useFallOff;
    
    public DrawMode drawMode;
    public Noise.NormalizeMode normalizeMode;

    
    public bool useFlatShading;

    [Range(0,6)]
    public int editPreviewLOD;

    //Perlin Noise Parameters
    public float noiseScale;
    public int octaves;
    [Range(0, 1)]
    public float persistence;
    public float lacunarity;

    //Height modificator
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public int seed;
    public Vector2 offset;

    //Allow for update in preview mode
    public bool autoUpdate;

    public TerrainType[] regions;
    static MapGenerator instance;


    float[,] falloffMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>(); 
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();


    //Tree Spawn
    [HideInInspector]
    public MapDisplay display;
    [HideInInspector]
    public Vector3[] verticesList;

    void Awake() {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
        
    }

    public static int mapChunkSize {
        get {
            if (instance == null)
                instance = FindObjectOfType<MapGenerator>();

            if (instance.useFlatShading)
                return 95;
            else
                return 239;
        }
    }

    MapData GenerateMapData(Vector2 centre) {
        //Sum + 2 to sizes to compensate for the border, we generate one extra noise value on left, right, top and bottom
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseScale, seed, octaves, persistence, lacunarity, centre + offset, normalizeMode);

        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];

        for (int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++){
                if (useFallOff)
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                

                float currentHeight = noiseMap[x, y];

                for (int i = 0; i < regions.Length; i++) {
                    if (currentHeight >= regions[i].height)
                        colourMap[y * mapChunkSize + x] = regions[i].colour;
                    else 
                        break;
                }
            }
        }

        return new MapData(noiseMap, colourMap);
    }

    //Threading
    public void RequestMapData(Vector2 centre,Action<MapData> callback) {
        ThreadStart threadStart = delegate {
            MapDataThread(centre,callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre , Action<MapData> callback) {
        //When calling a method from inside a thread, the method will run on the thread
        MapData mapData = GenerateMapData(centre);
        lock (mapDataThreadInfoQueue){
            //The lock will block the thread so that no other process can call it while it's been already called
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod,Action<MeshData> callback) {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod, useFlatShading);

        lock (meshDataThreadInfoQueue) {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update() {
        if (mapDataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++) {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    public void DrawMapInEditor() {
        //Display method
        MapData mapData = GenerateMapData(Vector2.zero);

        display = FindObjectOfType<MapDisplay>();

        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap,
                meshHeightMultiplier,
                meshHeightCurve,
                editPreviewLOD, useFlatShading), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
            verticesList = display.meshData.vertices;
        }
        else if (drawMode == DrawMode.FalloffMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
            verticesList = display.meshData.vertices;
        }

    }



    private void OnValidate() {
        if (lacunarity < 1) 
            lacunarity = 1;
        
        if (octaves < 0)
            octaves = 0;

        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);

    }

    struct MapThreadInfo<T>{
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }

    
}

[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color colour;
}

public struct MapData {
    public readonly float[,] heightMap;
    public readonly Color[] colourMap;

    public MapData(float[,] heightMap, Color[] colourMap){
        this.heightMap = heightMap;
        this.colourMap = colourMap;
    }

}