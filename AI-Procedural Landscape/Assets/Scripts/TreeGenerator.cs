using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGenerator : MonoBehaviour
{
    /*
    MapGenerator mapGenerator;
    public ObjectType[] Spawner;
    Vector3[] vertices;
    public float previewScaleFactor;

    void Start() {
        
    }

    public void generateTrees(float scaleFactor = 1)
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        vertices = mapGenerator.verticesList;
        Debug.Log(vertices.Length);
        Texture2D noiseMapTexture = Noise.QuickNoiseMap(241, 241, 500);

        Debug.Log(vertices.Length);

        //Check if Spawner list is empty
        if (Spawner.Length > 0) {
            for (int j = 0; j < Spawner.Length; j++) {

                //Check if object list is empty -> Prevent Null Reference
                if (Spawner[j].objectList.Count > 0)
                {

                    //Delete Objects if they exist already
                    if (GameObject.Find(Spawner[j].name))
                        DestroyImmediate(GameObject.Find(Spawner[j].name));

                    //Create Parent Object
                    GameObject parent = new GameObject(Spawner[j].name);

                    for (int i = 0; i < vertices.Length - 1; i++) {
                        //In order to avoid 2 for loops, we use a random value from 1 to i index to calculate the noise
                        //and then compare it with the density value of each element
                        float noiseMapValue = noiseMapTexture.GetPixel(i, Random.Range(1, i)).g;

                        if (noiseMapValue > 1 - Spawner[j].density) {

                            if (vertices[i].y >= Spawner[j].minPositionHeight &&
                                vertices[i].y <= Spawner[j].maxPositionHeight) {

                                int randomPrefab = Random.Range(0, Spawner[j].objectList.Count - 1);

                                //Add a random number from a certain range to make the  spawning more natural
                                Vector3 pos = new Vector3(vertices[i].x + Random.Range(-0.5f, 0.5f), vertices[i].y, vertices[i].z + Random.Range(-0.5f, 0.5f)) * scaleFactor;

                                GameObject go = Instantiate(Spawner[j].objectList[randomPrefab],
                                                pos,
                                                Quaternion.Euler(new Vector3(Random.Range(-10, 10), Random.Range(0, 360), Random.Range(-10, 10))),
                                                parent.transform);

                                go.transform.localScale = Vector3.one * Random.Range(Spawner[j].objectMinSize, Spawner[j].objectMaxSize);
                            }
                        }
                    }
                }
            }
        }
    }
}

[System.Serializable]
public struct ObjectType
{
    public string name;

    public List<GameObject> objectList;

    public float density;

    public float objectMinSize;
    public float objectMaxSize;

    public float minPositionHeight;
    public float maxPositionHeight;

    */
}
