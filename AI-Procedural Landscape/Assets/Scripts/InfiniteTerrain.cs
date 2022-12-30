using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    const float scale = 2f;
    //This variable sets a threshold the viewer has to move until we update the chunks
    //This way, chunks do not have to update at every frame
    const float viewerMoveThresholdChunkUpdate = 25f;

    const float sqrViewerMoveThresholdChunkUpdate = viewerMoveThresholdChunkUpdate * viewerMoveThresholdChunkUpdate;

    public LodInfo[] detailLevels;
    //Set max View Distance as static so it can be changed at runtime
    public static float maxViewDistance;

    public Transform viewer;
    public Material mapMapterial;

    public static Vector2 viewerPosition;
    Vector2 prevViewerPosition;

    int chunkSize;
    int chunksVisibleInViewDistance;

    static MapGenerator mapGenerator;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();

    static List<TerrainChunk> terrainChunksVisibleLUpdate = new List<TerrainChunk>();


    void Start() {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;

        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();
    }

    private void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;

        if ((prevViewerPosition - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdChunkUpdate) {
            prevViewerPosition = viewerPosition;
            UpdateVisibleChunks();
        }
        
    }

    void UpdateVisibleChunks() {

        for (int i = 0; i < terrainChunksVisibleLUpdate.Count; i++) {
            terrainChunksVisibleLUpdate[i].setVisible(false);
        }

        terrainChunksVisibleLUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++) {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                //Spawn terrain chunk on surounding coordinates
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                //Check if chunk exists already
                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)) {
                    terrainChunkDictionary[viewedChunkCoord].UpdateChunk();

                    
                }
                else {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMapterial));
                }
            }
        }
    }

    public class TerrainChunk {
        GameObject meshObject;
        Vector2 position;

        //Use Bounds to find the distance between a given point and the viewer's position
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LodInfo[] detailLevels;
        LODMesh[] detailLevelsMeshes;
        LODMesh collissionLODMesh;

        MapData mapData;
        bool mapDataReceived;

        int prevLODIndex = -1;

       

        public TerrainChunk(Vector2 coordinate, int size, LodInfo[] detailLevels, Transform parent, Material material) {
            this.detailLevels = detailLevels;

            position = coordinate * size;
            bounds = new Bounds(position, Vector2.one * size);

            Vector3 positionVector3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();

            meshRenderer.material = material;
            meshObject.transform.position = positionVector3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;
            //Let the Update method make it visible
            setVisible(false);

            detailLevelsMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++) {
                detailLevelsMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateChunk);

                if (detailLevels[i].userForCollider) {
                    collissionLODMesh = detailLevelsMeshes[i];
                }

            }

            mapGenerator.RequestMapData(position,OnMapDataReceived);
        }

       
        void OnMapDataReceived(MapData mapData) {
            //Testing purposes
            //print("Map Data received");

            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;
            UpdateChunk();
        }

        //Find the distance from the viewer so that it enables the mesh object
        public void UpdateChunk() {
            if (mapDataReceived) {
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

                if (visible){
                    int lodIndex = 0;

                    for (int i = 0; i < detailLevels.Length; i++){
                        if (viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                            lodIndex = i + 1;
                        else
                            break;
                    }

                    if (lodIndex != prevLODIndex){
                        LODMesh lodMesh = detailLevelsMeshes[lodIndex];
                        if (lodMesh.hasMesh){
                            prevLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;

                        }
                        else if (!lodMesh.hasRequestedMesh){
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                    if (lodIndex == 0) {
                        if (collissionLODMesh.hasMesh){
                            meshCollider.sharedMesh = collissionLODMesh.mesh;
                        }
                        else if (!collissionLODMesh.hasRequestedMesh) {
                            collissionLODMesh.RequestMesh(mapData);
                        }
                    }

                    terrainChunksVisibleLUpdate.Add(this);

                }

                setVisible(visible);
            }
        }

        public void setVisible(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool isVisible() {
            return meshObject.activeSelf;
        }
    }


    class LODMesh{
        public Mesh mesh;
        //Keep track of whether or not we requested the mesh yet
        public bool hasRequestedMesh;
        public bool hasMesh;

        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback) {
            this.updateCallback = updateCallback;
            this.lod = lod;
        }

        void OnMeshDataReceived(MeshData meshData) {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData) {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LodInfo {
        public int lod;
        //Distance within the lod is active
        //Once the viewer is outside the distance threshold, this will show the lower lod
        public float visibleDistanceThreshold;

        //This parameter is useful for selecting which level of detail is the one for the collission
        public bool userForCollider;
    }
}
