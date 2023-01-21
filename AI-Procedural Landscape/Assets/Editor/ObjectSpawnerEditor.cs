using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ObjectSpawner))]
public class ObjectSpawnerEditor : Editor {
    
    public override void OnInspectorGUI() {

        MeshData meshData = FindObjectOfType<MapPreview>().generatedMeshData;
        ObjectSpawner objectSpawner = (ObjectSpawner)target;
        if (DrawDefaultInspector())
        {

        }

        if (GUILayout.Button("Generate"))
        {
           //objectSpawner.generateObjects(meshData, new Vector2(0,0));
        }
    }
}
