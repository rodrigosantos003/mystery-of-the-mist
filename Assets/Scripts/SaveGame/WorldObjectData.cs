using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// World Object data to save
/// </summary>
[System.Serializable]
public class WorldObjectData
{
    public ObjectPosition position;
    public ObjectRotation rotation;
    public string type; // Type of world object

    [Serializable]
    public class ObjectPosition {
        public float X;
        public float Y;
        public float Z;
    }
    
    [Serializable]
    public class ObjectRotation {
        public float X;
        public float Y;
        public float Z;
    }
    public WorldObjectData(GameObject obj) {
        Vector3 objPosition = obj.transform.position;
        Vector3 objRotation = obj.transform.eulerAngles;
        
        position = new ObjectPosition
        {
            X = objPosition.x,
            Y = objPosition.y,
            Z = objPosition.z
        };
        
        rotation = new ObjectRotation
        {
            X = objRotation.x,
            Y = objRotation.y,
            Z = objRotation.z
        };

        type = obj.name.Substring(0, obj.name.Length - 7);
    }
}

/// <summary>
/// Class to wrap the list of objetcs to save
/// </summary>
[System.Serializable]
public class WorldObjectDataWrapper
{
    public WorldObjectData[] worldObjects;

    public WorldObjectDataWrapper(int length)
    {
        worldObjects = new WorldObjectData[length];
    }
}