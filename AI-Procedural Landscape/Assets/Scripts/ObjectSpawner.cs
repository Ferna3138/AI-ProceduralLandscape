using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour {
    Vector3[] vertices;

    [HideInInspector]
    public List<GameObject> visibleParentsList = new List<GameObject>();

    public void Start() {
        destroyObjects(Vector2.zero);
        visibleParentsList = new List<GameObject>();
    }
    public void generateObjects(SpawnSettings objectSpawn , MeshData meshData, Vector2 coordinates, float scaleFactor) {

        vertices = meshData.vertices;
        
        Texture2D noiseMapTexture = Noise.QuickNoiseMap(241, 241, 500);

        //Check if Spawner list is empty
        if (objectSpawn.Spawner.Length > 0) {
            for (int j = 0; j < objectSpawn.Spawner.Length; j++) {

                //Check if object list is empty -> Prevent Null Reference
                if (objectSpawn.Spawner[j].objectList.Count > 0) {

                    //Create Parent Object
                    GameObject parent = new GameObject(objectSpawn.Spawner[j].name + coordinates);
                    parent.transform.localPosition = new Vector3(coordinates.x, 0, coordinates.y);
                    
                    visibleParentsList.Add(parent);

                    for (int i = 0; i < vertices.Length - 1; i++) {
                        float noiseMapValue = noiseMapTexture.GetPixel(i, Random.Range(1, i)).g;

                        if (noiseMapValue > 1 - Random.Range(0.0f, objectSpawn.Spawner[j].density)) {
                            if (vertices[i].y >= (objectSpawn.Spawner[j].minPositionHeight + Random.Range(0,10)) &&
                                vertices[i].y <= (objectSpawn.Spawner[j].maxPositionHeight) + Random.Range(0,10)) {

                                int randomPrefab = Random.Range(0, objectSpawn.Spawner[j].objectList.Count - 1);

                                //Add a random number within a certain range to make the spawning more natural
                                Vector3 pos = new Vector3(vertices[i].x + Random.Range(-1f, 1f) + coordinates.x, vertices[i].y, vertices[i].z + Random.Range(-1f, 1f) + coordinates.y);

                                GameObject go = Instantiate(objectSpawn.Spawner[j].objectList[randomPrefab],
                                                pos * scaleFactor,
                                                Quaternion.Euler(new Vector3(Random.Range(-10, 10), Random.Range(0, 360), Random.Range(-10, 10))),
                                                parent.transform);

                                go.transform.localScale = Vector3.one * Random.Range(objectSpawn.Spawner[j].objectMinSize, objectSpawn.Spawner[j].objectMaxSize);

                            }
                        }
                    }
                }
            }
        }
    }

    public void setVisibleObjects(bool visible, Vector2 coordinates) {
        foreach (GameObject var in visibleParentsList) {
            if (var.transform.position == new Vector3(coordinates.x, 0, coordinates.y)) {
                var.SetActive(visible);
            }
        }
    }

    public void destroyObjects(Vector2 coordinates) {
        if (visibleParentsList.Count > 1) {
            foreach (GameObject var in visibleParentsList)
                DestroyImmediate(var);
        }
    }

}
