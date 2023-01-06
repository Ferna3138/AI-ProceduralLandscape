using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TreeGenerator))]
public class TreeGeneratorEditor : Editor
{


    public override void OnInspectorGUI()
    {
        TreeGenerator treeGen = (TreeGenerator)target;
        if (DrawDefaultInspector())
        {

        }

        if (GUILayout.Button("Generate"))
        {
            //treeGen.generateTrees(treeGen.previewScaleFactor);
        }
    }
}
