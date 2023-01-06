using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : UpdatableData{

    public const int numSupportedLODs = 5;
    public const int numSupportedChunkSizes = 9; //Length of Supported Chunk Sizes
    public const int numSupportedFlatshadedChunkSizes = 3; //Length of Supported Flat Shaded Chunk Sizes

    //This values must be divisible by all the LOD levels
    public static int[] supportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

    public float meshScale = 2.5f;
    public bool useFlatShading;

    [Range(0, numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;
    [Range(0, numSupportedFlatshadedChunkSizes - 1)]
    public int flatShadedChunkSizeIndex;


    //Number of vertexes per line of mesh rendered at LOD = 0
    //Includes the 2 extra vertees that are excluded from final mesh but used for calculating normals
    public int numberVertexPerLine {
        get {
            return supportedChunkSizes[(useFlatShading) ? flatShadedChunkSizeIndex : chunkSizeIndex] + 5;
        }
    }

    public float meshWorldSize {
        get {
            return (numberVertexPerLine - 3) * meshScale;
        }
    }
}
