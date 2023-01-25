using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ObjectSpawner))]
public class ObjectSpawnerEditor : Editor {
    
    public override void OnInspectorGUI() {

        MeshData meshData = FindObjectOfType<MapPreview>().generatedMeshData;
        SpawnSettings spawnSettings = FindObjectOfType<MapPreview>().objectSpawn;
        ObjectSpawner objectSpawner = (ObjectSpawner)target;
        if (DrawDefaultInspector())
        {

        }

        if (GUILayout.Button("Generate"))
        {
            objectSpawner.destroyObjects(Vector2.zero);
           objectSpawner.generateObjects(spawnSettings,meshData, Vector2.zero, 2f);
        }
    }
}
