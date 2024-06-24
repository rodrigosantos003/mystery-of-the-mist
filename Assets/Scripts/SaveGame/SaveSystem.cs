using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SaveSystem
{
    [Serializable]
    public class SaveData
    {
        public PlayerData playerData;
        public EnemyDataWrapper enemyData;
        public WorldObjectDataWrapper worldObjectData;
        public int currentLevel;
    }
    
    public static void Save(PlayerController player, GameObject[] enemies, GameObject[] worldObjects, int level)
    {
        SaveData data = new SaveData();
        
        data.playerData = new PlayerData(player);
        data.enemyData = new EnemyDataWrapper(enemies.Length);
        data.worldObjectData = new WorldObjectDataWrapper(worldObjects.Length);
        data.currentLevel = level;
        
        for(int i = 0; i < enemies.Length; i++)
        {
            data.enemyData.enemies[i] = new EnemyData(enemies[i].GetComponent<Enemy>());
        }
        
        for(int i = 0; i < worldObjects.Length; i++)
        {
            data.worldObjectData.worldObjects[i] = new WorldObjectData(worldObjects[i]);
        }
        
        string path = Application.persistentDataPath + "/save.json";

        string json = JsonUtility.ToJson(data);

        File.WriteAllText(path, json);

        Debug.Log("Game saved (" + path + ")");
        
    }

    /// <summary>
    /// Loads the saved Player object from the JSON file
    /// </summary>
    /// <returns></returns>
    public static PlayerData LoadPlayer()
    {
        string path = Application.persistentDataPath + "/save.json";

        if (!File.Exists(path))
        {
            Debug.LogError("Save file not found");
            return null;
        }

        string josn = File.ReadAllText(path);
        
        SaveData data = JsonUtility.FromJson<SaveData>(josn);
        
        PlayerData playerData = data.playerData;

        return playerData;
    }

    /// <summary>
    /// Loads the saved Enemies from the JSON File
    /// </summary>
    /// <returns></returns>
    public static EnemyDataWrapper LoadEnemies()
    {
        string path = Application.persistentDataPath + "/save.json";

        if (!File.Exists(path))
        {
            Debug.LogError("Save file not found");
            return null;
        }

        string json = File.ReadAllText(path);
        
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        
        EnemyDataWrapper enemyData = data.enemyData;

        return enemyData;
    }

    /// <summary>
    /// Loads the saved World Objects from the JSON file
    /// </summary>
    /// <returns></returns>
    public static WorldObjectDataWrapper LoadWorldObjects()
    {
        string path = Application.persistentDataPath + "/save.json";

        if (!File.Exists(path))
        {
            Debug.LogError("Save file not found");
            return null;
        }

        string json = File.ReadAllText(path);

        SaveData data = JsonUtility.FromJson<SaveData>(json);
        
        WorldObjectDataWrapper worldObjectData = data.worldObjectData;

        return worldObjectData;
    }
    
    public static int LoadLevel()
    {
        string path = Application.persistentDataPath + "/save.json";

        if (!File.Exists(path))
        {
            Debug.LogError("Save file not found");
            return 0;
        }

        string json = File.ReadAllText(path);

        SaveData data = JsonUtility.FromJson<SaveData>(json);
        
        return data.currentLevel;
    }
}
