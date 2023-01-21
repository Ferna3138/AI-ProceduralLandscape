using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ObjectSpawn : UpdatableData {
    public ObjectType[] Spawner;

}

[System.Serializable]
public struct ObjectType {
    public string name;

    public List<GameObject> objectList;

    public float density;

    public float objectMinSize;
    public float objectMaxSize;

    public float minPositionHeight;
    public float maxPositionHeight;
}