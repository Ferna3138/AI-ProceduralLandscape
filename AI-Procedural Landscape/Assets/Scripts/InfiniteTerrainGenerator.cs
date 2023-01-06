using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrainGenerator : MonoBehaviour {
    
    //This variable sets a threshold the viewer has to move until we update the chunks
    //This way, chunks do not have to update at every frame
    const float viewerMoveThresholdChunkUpdate = 25f;
    const float sqrViewerMoveThresholdChunkUpdate = viewerMoveThresholdChunkUpdate * viewerMoveThresholdChunkUpdate;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;

    public int colliderLODIndex;
    public LodInfo[] detailLevels;


    public Transform viewer;
    public Material mapMaterial;

    Vector2 viewerPosition;
    Vector2 prevViewerPosition;

    float meshWorldSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();

    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();


    void Start() {
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

        for (int i = visibleTerrainChunks.Count - 1; i >=  0; i--) {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coordinate);
            visibleTerrainChunks[i].UpdateChunk();
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
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, meshWorldSize, detailLevels, colliderLODIndex, transform, viewer, mapMaterial);
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);

                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;

                        newChunk.Load();
                    }
                }
            }
        }
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