using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {
    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    private InfiniteTerrainGenerator infiniteTerrain;
    private NoiseSettings noiseSettings;
    public Transform viewer;

    Vector2 viewerPosition;

    public Vector2 gridWorldSize;
    Node[,] grid;
    public float nodeRadius;
    public LayerMask unwalkableMask;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    HeightMap heightMap;

    private void Start() { 
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        //meshSettings = infiniteTerrain.meshSettings;
        //heightMapSettings = infiniteTerrain.heightMapSettings;
        //noiseSettings = heightMapSettings.noiseSettings;

        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);


        //heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numberVertexPerLine, meshSettings.numberVertexPerLine, heightMapSettings, viewerPosition);

        //CreateGrid();

    }

    void CreateGrid() {
        grid = new Node[meshSettings.numberVertexPerLine, meshSettings.numberVertexPerLine];

        Vector2 topLeft = new Vector2(-1, 1) * meshSettings.meshWorldSize / 2f;

        for (int x = 0; x < meshSettings.numberVertexPerLine; x++) {
            for (int y = 0; y < meshSettings.numberVertexPerLine; y++) {
                
                
                //Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius); ;
                //bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                float height = heightMap.values[x,y];

                Vector2 percent = new Vector2(x - 1, y - 1) / (meshSettings.numberVertexPerLine - 3);
                Vector2 vertexPosition2D = topLeft + new Vector2(percent.x, -percent.y) * meshSettings.meshWorldSize;

                //worldPoint.y = height;
                Vector3 worldPoint = new Vector3(vertexPosition2D.x, height, vertexPosition2D.y);
                grid[x, y] = new Node(true, worldPoint);

                //Vector3 pos = 
                //grid[x,y] = new Node(walkable, )
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null) {
            foreach (Node n in grid) {
                Gizmos.color = (n.walkable) ? Color.white : Color.red; 
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}
