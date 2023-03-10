using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, MeshSettings meshSettings, int levelOfDetail) {

        int skipIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int numberVertexPerLine = meshSettings.numberVertexPerLine;

        Vector2 topLeft = new Vector2(-1, 1) * meshSettings.meshWorldSize / 2f;

        MeshData meshData = new MeshData(numberVertexPerLine, skipIncrement);

        int[,] vertexIndecesMap = new int[numberVertexPerLine, numberVertexPerLine];
        int meshVertexIndex = 0;
        int outOfMeshVertexIndex = -1;

        for (int y = 0; y < numberVertexPerLine; y++){
            for (int x = 0; x < numberVertexPerLine; x++){
                bool isOutOfMeshVertex = y == 0 || y == numberVertexPerLine - 1 || x == 0 || x == numberVertexPerLine - 1;
                bool isSkippedVertex = x > 2 &&
                    x < numberVertexPerLine - 3 &&
                    y > 2 &&
                    y < numberVertexPerLine - 3 &&
                    ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);


                if (isOutOfMeshVertex){
                    vertexIndecesMap[x, y] = outOfMeshVertexIndex;
                    outOfMeshVertexIndex--;
                }
                else if(!isSkippedVertex){
                    vertexIndecesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++; 
                }
            }
        }

        for (int y = 0; y < numberVertexPerLine; y++) {
            for (int x = 0; x < numberVertexPerLine; x++) {

                bool isSkippedVertex = x > 2 &&
                    x < numberVertexPerLine - 3 &&
                    y > 2 &&
                    y < numberVertexPerLine - 3 &&
                    ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);

                if (!isSkippedVertex) {
                    bool isOutOfMeshVertex = y == 0 || y == numberVertexPerLine - 1 || x == 0 || x == numberVertexPerLine - 1;
                    bool isMeshEdgeVertex = y == 1 || y == numberVertexPerLine - 2 || x == 1 || x == numberVertexPerLine - 2 && !isOutOfMeshVertex;
                    bool isMainVertex = (x - 2) % skipIncrement == 0 && (y - 2) % skipIncrement == 0 && !isOutOfMeshVertex && !isMeshEdgeVertex;
                    bool isEdgeConnectionVertex = (y == 2 || y == numberVertexPerLine - 3 || x == 2 || x == numberVertexPerLine - 3) && !isOutOfMeshVertex && !isMeshEdgeVertex && !isMainVertex;

                    int vertexIndex = vertexIndecesMap[x, y];

                    //Vertexes Creation
                        //We want the percent = 0 when x = 1 which is where the mesh actually starts, and percent = 1 when x = numVericesPerLine - 2 which is where the mesh ends
                    Vector2 percent = new Vector2(x - 1, y - 1) / (numberVertexPerLine - 3);
                    
                    Vector2 vertexPosition2D = topLeft + new Vector2(percent.x, -percent.y) * meshSettings.meshWorldSize;
                    float height = heightMap[x, y];

                    if (isEdgeConnectionVertex) {
                        bool isVertical = x == 2 || x == numberVertexPerLine - 3;

                        int dstToMainVertexA = ((isVertical) ? y - 2 : x - 2) % skipIncrement;
                        int dstToMainVertexB = skipIncrement - dstToMainVertexA;
                        float dstPercentFromAtoB = dstToMainVertexA / (float) skipIncrement;

                        float heightMainVertexA = heightMap[(isVertical) ? x : x - dstToMainVertexA, (isVertical) ? y - dstToMainVertexA : y];
                        float heightMainVertexB = heightMap[(isVertical) ? x : x + dstToMainVertexB, (isVertical) ? y + dstToMainVertexB : y];

                        height = heightMainVertexA * (1 - dstPercentFromAtoB) + heightMainVertexB * dstPercentFromAtoB;
                    }

                    meshData.AddVertex(new Vector3(vertexPosition2D.x, height, vertexPosition2D.y), percent, vertexIndex);

                    bool createTriangle = x < numberVertexPerLine - 1 && y < numberVertexPerLine - 1 && (!isEdgeConnectionVertex || (x != 2 && y != 2));
                    
                    if (createTriangle) {
                        int currentIncrement = (isMainVertex && x != numberVertexPerLine - 3 && y != numberVertexPerLine - 3) ? skipIncrement : 1;
                        int a = vertexIndecesMap[x, y];
                        int b = vertexIndecesMap[x + currentIncrement, y];
                        int c = vertexIndecesMap[x, y + currentIncrement];
                        int d = vertexIndecesMap[x + currentIncrement, y + currentIncrement];

                        meshData.AddTriangle(a, d, c);
                        meshData.AddTriangle(d, a, b);
                    }
                }
            }
        }

        meshData.BakeNormals();

        return meshData;
    }
}

public class MeshData {
    public Vector3[] vertices;
    
    int[] triangles;
    Vector2[] uvs;
    Vector3[] bakedNormals;
    Vector3[] outOfMeshVertices;
    int[] outOfMeshTriangles;

    int triangleIndex;
    int outOfMeshTriangleIndex;

    public MeshData(int numVertsPerLine, int skipIncrement) {
        int numMeshEdgeVertices = (numVertsPerLine - 2) * 4 - 4;
        int numEdgeConnectionVertices = (skipIncrement - 1) * (numVertsPerLine - 5) / skipIncrement * 4;
        int numMainVertiesPerLine = (numVertsPerLine - 5) / skipIncrement * 4 + 1;
        int numMainVertices = numMainVertiesPerLine * numMainVertiesPerLine;

        vertices = new Vector3[numMeshEdgeVertices + numEdgeConnectionVertices + numMainVertices];
        uvs = new Vector2[vertices.Length];

        int numMeshEdgeTriangles = 8 * (numVertsPerLine - 4);
        int numMainTriangles = (numMainVertiesPerLine - 1) * (numMainVertiesPerLine - 1) * 2;
        triangles = new int[(numMeshEdgeTriangles + numMainTriangles) * 3];

        outOfMeshVertices = new Vector3[numVertsPerLine * 4 - 4];
        outOfMeshTriangles = new int[24 * (numVertsPerLine - 2)];
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex) {
        if (vertexIndex < 0){
            outOfMeshVertices[-vertexIndex - 1] = vertexPosition;
        }
        else {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c) {
        //Check if the vertexes are border vertexes
        if (a < 0 || b < 0 || c < 0) {
            outOfMeshTriangles[outOfMeshTriangleIndex] = a;
            outOfMeshTriangles[outOfMeshTriangleIndex + 1] = b;
            outOfMeshTriangles[outOfMeshTriangleIndex + 2] = c;

            outOfMeshTriangleIndex += 3;
        }
        else {
            triangles [triangleIndex] = a;
            triangles [triangleIndex + 1] = b;
            triangles [triangleIndex + 2] = c;

            triangleIndex += 3;
        }
    }

    Vector3[] CalculateNormal() {
        Vector3[] vertexNormals = new Vector3[vertices.Length];

        //We get the number of trinalges by dividing the length by 3
        int triangleCount = triangles.Length / 3;

        //if i is the triangle we're working with then i * 3 is the index of the triangle in the Array
        for (int i = 0; i < triangleCount; i++) {
            int normalTriangleIndex = i * 3;

            //We want the indexes of all the vertexes that make the current triangle
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndexes(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals [vertexIndexA] += triangleNormal;
            vertexNormals [vertexIndexB] += triangleNormal;
            vertexNormals [vertexIndexC] += triangleNormal;
        }

        int borderTriangleCount = outOfMeshTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++){
            int normalTriangleIndex = i * 3;
            int vertexIndexA = outOfMeshTriangles [normalTriangleIndex];
            int vertexIndexB = outOfMeshTriangles [normalTriangleIndex + 1];
            int vertexIndexC = outOfMeshTriangles [normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndexes(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0) {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0) {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if(vertexIndexC >= 0) {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }


        for (int i = 0; i < vertexNormals.Length; i++) {
            vertexNormals[i].Normalize();
        }
        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndexes(int indexA, int indexB, int indexC) {
        Vector3 pointA = (indexA < 0) ? outOfMeshVertices[-indexA - 1] : vertices [indexA];
        Vector3 pointB = (indexB < 0) ? outOfMeshVertices[-indexB - 1] : vertices [indexB];
        Vector3 pointC = (indexC < 0) ? outOfMeshVertices[-indexC - 1] : vertices [indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }


    public void BakeNormals() {
        bakedNormals = CalculateNormal();
    }



    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.normals = bakedNormals;
        
        return mesh;
    }

}