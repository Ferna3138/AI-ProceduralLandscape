using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class InfiniteTerrainGenerator : MonoBehaviour {
    
    //This variable sets a threshold the viewer has to move until we update the chunks
    //This way, chunks do not have to update at every frame
    const float viewerMoveThresholdChunkUpdate = 25f;
    const float sqrViewerMoveThresholdChunkUpdate = viewerMoveThresholdChunkUpdate * viewerMoveThresholdChunkUpdate;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;
    public SpawnSettings objectSpawn;

    public Material waterMaterial;
    public float waterHeight;

    public int colliderLODIndex;
    public LodInfo[] detailLevels;


    public Transform viewer;
    public Material mapMaterial;

    Vector2 viewerPosition;
    Vector2 prevViewerPosition;

    float meshWorldSize;
    int chunksVisibleInViewDistance;

    //Terrain Chunks
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    //Water Chunks
    Dictionary<Vector2, TerrainChunk> waterChunksDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleWaterChunks = new List<TerrainChunk>();

    public bool spawnObjects;
    ObjectSpawner objectSpawner;

    void Start() {
        objectSpawner = FindObjectOfType<ObjectSpawner>();

        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        float maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;

        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / meshWorldSize);

        UpdateVisibleChunks();
        
    }

    private void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if (viewerPosition != prevViewerPosition) {
            foreach (TerrainChunk chunk in visibleTerrainChunks) {
                chunk.UpdateCollider();
            }
        }

        if ((prevViewerPosition - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdChunkUpdate) {
            prevViewerPosition = viewerPosition;
            UpdateVisibleChunks();
        }
    }


    void UpdateVisibleChunks() {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        HashSet<Vector2> alreadyUpdatedWaterChunkCoords = new HashSet<Vector2>();

        for (int i = visibleTerrainChunks.Count - 1; i >=  0; i--) {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coordinate);
            visibleTerrainChunks[i].UpdateChunk();

        }

        for (int i = visibleWaterChunks.Count - 1; i >= 0; i--) {
            alreadyUpdatedWaterChunkCoords.Add(visibleWaterChunks[i].coordinate);
            visibleWaterChunks[i].UpdateWaterChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++) {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++) {
                //Spawn terrain chunk on surounding coordinates
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                
                //Check if chunk has been updated already
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord)) {
                    //Check if chunk exists already
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)) {
                        terrainChunkDictionary[viewedChunkCoord].UpdateChunk();
                    }
                    else {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, meshWorldSize, detailLevels, colliderLODIndex, transform, viewer, mapMaterial,objectSpawner, objectSpawn, spawnObjects);
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;

                        newChunk.Load();
                    }
                }

                if (!alreadyUpdatedWaterChunkCoords.Contains(viewedChunkCoord)) {
                    if (waterChunksDictionary.ContainsKey(viewedChunkCoord)) {
                        waterChunksDictionary[viewedChunkCoord].UpdateWaterChunk();
                    }
                    else {
                        TerrainChunk waterChunk = new TerrainChunk(viewedChunkCoord, meshWorldSize, detailLevels, meshSettings, waterHeight, transform, viewer, waterMaterial);
                        waterChunksDictionary.Add(viewedChunkCoord, waterChunk);
                        waterChunk.onVisibilityChanged += OnWaterVisibilityChanged;

                        waterChunk.UpdateChunk();
                    }
                }
            }
        }
    }


    void OnWaterVisibilityChanged(TerrainChunk chunk, bool isVisible) {
        if (isVisible)
            visibleWaterChunks.Add(chunk);
        else
            visibleWaterChunks.Remove(chunk);
    }

    void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible) {
        if (isVisible) {
            visibleTerrainChunks.Add(chunk);
        }
        else {
            visibleTerrainChunks.Remove(chunk);
        }
    }


}



[System.Serializable]
public struct LodInfo {

    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int lod;

    //Distance within the lod is active
    //Once the viewer is outside the distance threshold, this will show the lower lod
    public float visibleDistanceThreshold;

    public float sqrVisibleDistanceThreshold {
        get {
            return visibleDistanceThreshold * visibleDistanceThreshold;
        }
    }
}

