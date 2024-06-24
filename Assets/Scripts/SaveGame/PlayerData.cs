using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Player data to save
/// </summary>
[System.Serializable]
public class PlayerData
{
    public PlayerPosition Position;
    public PlayerRotation Rotation;
    public SavedAttack[] Attacks;
    public int MaxHealth;
    public int Damage;
    public int MovementRange;
    public int AttackRange;
    public float CurrentHealth;

    [Serializable]
    public class PlayerPosition
    {
        public float x;
        public float y;
        public float z;
    }
    
    [Serializable]
    public class PlayerRotation
    {
        public float X;
        public float Y;
        public float Z;
        public float W;
    }
    
    [Serializable]
    public class SavedAttack
    {
        public string AttackName;
        public int AttackLevel;
    }
    
    public PlayerData(PlayerController player)
    {   
        Vector3 playerPosition = player.transform.position;
        Quaternion playerRotation = player.transform.rotation;
        
        Position = new PlayerPosition
        {
            x = playerPosition.x,
            y = playerPosition.y,
            z = playerPosition.z
        };

        Rotation = new PlayerRotation
        {
            X = playerRotation.x,
            Y = playerRotation.y,
            Z = playerRotation.z,
            W = playerRotation.w
        };
        
        Attacks = new SavedAttack[player.Stats.Attacks.Count];
        
        for (int i = 0; i < player.Stats.Attacks.Count; i++)
        {
            Attacks[i] = new SavedAttack
            {
                AttackName = player.Stats.Attacks[i].AttackName,
                AttackLevel = player.Stats.Attacks[i].AttackLevel
            };
        }
        
        MaxHealth = player.Stats.MaxHealth;
        Damage = player.Stats.Damage;
        MovementRange = player.Stats.MovementRange;
        AttackRange = player.Stats.AttackRange;
        CurrentHealth = player.CurrentHealth;
    }
}
