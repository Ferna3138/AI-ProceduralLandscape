using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatableData : ScriptableObject{
    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

    //This makes so we only compile this methods if we're using the Unity Editor
    //If we use a compiled version, this gets ignored
#if UNITY_EDITOR
    protected virtual void OnValidate(){
        if (autoUpdate) {
            UnityEditor.EditorApplication.update += NotifyUpdateValues;
        }
    }

    public void NotifyUpdateValues (){
        UnityEditor.EditorApplication.update -= NotifyUpdateValues;
        if (OnValuesUpdated != null) {
            OnValuesUpdated();
        }

    }
#endif

}
