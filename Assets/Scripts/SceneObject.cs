using System;
using UnityEngine;

[Serializable]
class SceneObject
{
    public string id;
    public string name;
    public Vector3 referencePoint1;
    public Vector3 referencePoint2;
    public SceneItem[] items;
}