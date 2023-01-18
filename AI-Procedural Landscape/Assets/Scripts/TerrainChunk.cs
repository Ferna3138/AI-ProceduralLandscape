using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk {
    public event System.Action<TerrainChunk, bool> onVisibilityChanged;
    
    //This value establishes how close the player has to be to the next chunk in order to create the collider
    const float colliderGenerationDistanceThreshold = 5;

    public Vector2 coordinate;

    GameObject meshObject;
    Vector2 sampleCenter;

    //Use Bounds to find the distance between a given point and the viewer's position
    public Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    LodInfo[] detailLevels;
    LODMesh[] detailLevelsMeshes;
    int colliderLODIndex;

    HeightMap heightMap;
    bool heightMapReceived;
    int prevLODIndex = -1;

    bool hasSetCollider;
    float maxViewDistance;

    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;


    Transform viewer;
    public TerrainChunk(Vector2 coordinate, HeightMapSettings heightMapSettings, MeshSettings meshSettings, float meshWorldSize, LodInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material) {
        this.coordinate = coordinate;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;

        sampleCenter = coordinate * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 position = coordinate * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();

        meshRenderer.material = material;
        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;

        //Let the Update method make it visible
        setVisible(false);

        detailLevelsMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++) {
            detailLevelsMeshes[i] = new LODMesh(detailLevels[i].lod);
            detailLevelsMeshes[i].updateCallback += UpdateChunk;
            if (i == colliderLODIndex) {
                detailLevelsMeshes[i].updateCallback += UpdateCollider;
            }
        }

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        

    }

    public void Load() {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numberVertexPerLine, meshSettings.numberVertexPerLine, heightMapSettings, sampleCenter), OnHeightMapReceived);
    }


    void OnHeightMapReceived(object heightMapObject) {
        this.heightMap = (HeightMap)heightMapObject;
        heightMapReceived = true;

        UpdateChunk();
    }


    Vector2 viewerPosition {
        get {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    public void UpdateChunk() {
        if (heightMapReceived) {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

            bool wasVisible = isVisible();
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

            if (visible) {
                int lodIndex = 0;

                for (int i = 0; i < detailLevels.Length; i++) {
                    if (viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                        lodIndex = i + 1;
                    else
                        break;
                }

                if (lodIndex != prevLODIndex) {
                    LODMesh lodMesh = detailLevelsMeshes[lodIndex];
                    if (lodMesh.hasMesh) {
                        prevLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    }
                    else if (!lodMesh.hasRequestedMesh) {
                        lodMesh.RequestMesh(heightMap, meshSettings);
                    }
                }

            }

            if (wasVisible != visible) {
                setVisible(visible);
                if (onVisibilityChanged != null) {
                    onVisibilityChanged(this, visible);
                }

            }
        }
    }

    public void UpdateCollider() {
        if (!hasSetCollider) {
            float sqrDistanceFromViewerToEdge = bounds.SqrDistance(viewerPosition);

            if (sqrDistanceFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDistanceThreshold) {
                if (!detailLevelsMeshes[colliderLODIndex].hasRequestedMesh) {
                    detailLevelsMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
                }
            }

            if (sqrDistanceFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold) {
                if (detailLevelsMeshes[colliderLODIndex].hasMesh)
                    meshCollider.sharedMesh = detailLevelsMeshes[colliderLODIndex].mesh;
                hasSetCollider = true;
            }
        }
    }

    public void setVisible(bool visible) {
        meshObject.SetActive(visible);
    }

    public bool isVisible() {
        return meshObject.activeSelf;
    }



    //WATER CHUNK SECTION
    //Second Constructor
    public TerrainChunk(Vector2 coordinate, float meshWorldSize, LodInfo[] detailLevels, MeshSettings meshSettings, float waterHeight, Transform parent, Transform viewer, Material material)
    {
        this.coordinate = coordinate;
        this.viewer = viewer;

        sampleCenter = coordinate * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 position = coordinate * meshSettings.meshWorldSize;

        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

        meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        meshObject.name = "Water Chunk";
        //meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshObject.transform.localScale = new Vector3(meshSettings.meshWorldSize / 10, 1, meshSettings.meshWorldSize / 10);

        meshObject.GetComponent<Renderer>().material = material;
        meshObject.transform.position = new Vector3(position.x, waterHeight, position.y);
        meshObject.transform.parent = parent;

        //Let the Update method make it visible
        setVisible(false);

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
    }
    //Water Update method
    public void UpdateWaterChunk() {
        float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

        bool wasVisible = isVisible();
        bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

        if (wasVisible != visible)
        {
            setVisible(visible);
            if (onVisibilityChanged != null)
            {
                onVisibilityChanged(this, visible);
            }
        }
    }
}





class LODMesh {
    public Mesh mesh;
    //Keep track of whether or not we requested the mesh yet
    public bool hasRequestedMesh;
    public bool hasMesh;

    int lod;
    public event System.Action updateCallback;

    public LODMesh(int lod) {
        this.lod = lod;
    }

    void OnMeshDataReceived(object meshDataObject) {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasMesh = true;

        updateCallback();
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings) {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
    }
}
