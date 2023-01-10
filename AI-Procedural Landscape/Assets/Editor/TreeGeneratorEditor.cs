using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ObjectSpawner))]
public class ObjectSpawnerEditor : Editor
{


    public override void OnInspectorGUI()
    {
        ObjectSpawner objectSpawner = (ObjectSpawner)target;
        if (DrawDefaultInspector())
        {

        }

        if (GUILayout.Button("Generate"))
        {
            //treeGen.generateTrees(treeGen.previewScaleFactor);
        }
    }
}
