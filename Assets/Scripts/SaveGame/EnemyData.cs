using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy data to save
/// </summary>
[System.Serializable]
public class EnemyData
{
    [Serializable]
    public class EnemyPosition {
        public float X;
        public float Y;
        public float Z;
    }
    
    [Serializable]
    public class EnemyRotation {
        public float X;
        public float Y;
        public float Z;
        public float W;
    }
    public float health;
    public EnemyPosition position;
    public EnemyRotation rotation;
    public string type; // Type of enemy

    public EnemyData(Enemy enemy)
    {
        Vector3 objPosition = enemy.transform.position;
        Vector3 objRotation = enemy.transform.eulerAngles;
        
        position = new EnemyPosition
        {
            X = objPosition.x,
            Y = objPosition.y,
            Z = objPosition.z
        };
        
        rotation = new EnemyRotation
        {
            X = objRotation.x,
            Y = objRotation.y,
            Z = objRotation.z
        };

        type = enemy.name.Substring(0, enemy.name.Length - 7);
        
        health = enemy.GetCurrentHealth();
    }
}

/// <summary>
/// Class to wrap the list of enemies to save
/// </summary>
[System.Serializable]
public class EnemyDataWrapper
{
    public EnemyData[] enemies;

    public EnemyDataWrapper(int lenght)
    {
        enemies = new EnemyData[lenght];
    }
}