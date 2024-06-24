using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

[System.Serializable]
public class ObjectInWorld
{
    public GameObject prefab;
    public int minQuantity;
    public int maxQuantity;
}
public class ObjectSpawner : MonoBehaviour
{
    private BoxCollider _spawnArea;

    private bool _errorOccured;

    private float _height;
    
    private int rows;
    private int columns;
    
    private float cellSizeX;
    private float cellSizeZ;

    private int ObjectsSpawned = 0;
    private int ObjectsToSpawn = 0;

    private static bool debug;
    
    public List<ObjectInWorld> usedObjects;
    
    #region Setters
    
    public static void SetDebug(bool _debug)
    {
        debug = _debug;
    }

    public void SetCellSize(float x, float z)
    {
        cellSizeX = x;
        cellSizeZ = z;
        calculateRowsAndColumns();
    }
    
    private void calculateRowsAndColumns()
    {
        var bounds = _spawnArea ? _spawnArea.bounds : GetComponent<BoxCollider>().bounds;
        rows = Mathf.FloorToInt(bounds.size.z / cellSizeZ);
        columns = Mathf.FloorToInt(bounds.size.x / cellSizeX);
    }
    
    #endregion
    
    private void Start()
    {
        _spawnArea = GetComponent<BoxCollider>();
    }
    public IEnumerator StartSpawning(List<ObjectInWorld> objects)
    {
        usedObjects = objects;
        
        _spawnArea = GetComponent<BoxCollider>();

        List<int> spawnQuantity = new List<int>();
        
        foreach(var obj in objects)
        {
            var quantity = Random.Range(obj.minQuantity, obj.maxQuantity);
            spawnQuantity.Add(quantity);
            ObjectsToSpawn += quantity;
        }
        
        foreach (var obj in objects)
        {
            var quantity = spawnQuantity[objects.IndexOf(obj)];

            for (var i = 0; i < quantity; i++)
            {
                SpawnObject(obj.prefab);
            }
        }
        
        
        yield return new WaitUntil(() => ObjectsSpawned == ObjectsToSpawn);
    }

    private void SpawnObject(GameObject obj)
    {
        var spawnPosition = GenerateSpawnPosition(obj);

        if (_errorOccured)
        {
            Debug.LogError("Não foi encontrado um local para spawnar o objeto " + obj.name);
            return;
        }

        GameObject instance = Instantiate(obj, spawnPosition, Quaternion.identity, transform);

        AdjustObjectHeight(instance);
        
        Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        
        instance.transform.rotation = rotation;

        var agent = instance.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = true;
        }
        ObjectsSpawned++;
    }

    private Vector3 GenerateSpawnPosition(GameObject obj)
    {
        Vector3 spawnPosition;
        var bounds = _spawnArea.bounds;

        var attempts = 0;

        do
        {
            var row = Random.Range(0, rows);
            var column = Random.Range(0, columns);

            var x = bounds.min.x + (bounds.size.x / columns) * (column + 0.5f);
            var z = bounds.min.z + (bounds.size.z / rows) * (row + 0.5f);
            var y = 0;

            spawnPosition = new Vector3(x, y, z);

            if (attempts++ > 100)
            {
                _errorOccured = true;
                break;
            }
        }
        while (IsPositionOccupied(spawnPosition));

        return spawnPosition;
    }
    
    private bool IsPositionOccupied(Vector3 position)
    {
        Vector3 halfExtents = new Vector3(cellSizeX / 2.0f, 5, cellSizeZ / 2.0f);
        
        int layerMask = LayerMask.GetMask("Entity") | LayerMask.GetMask("Object");

        position.y = 0.1f;
        
        bool isOccupied = Physics.CheckBox(position, halfExtents, Quaternion.identity, layerMask);
        
        return isOccupied;
    }

    void OnDrawGizmos()
    {
        if (_spawnArea == null || !debug) return;

        Gizmos.color = Color.yellow;
        var bounds = _spawnArea.bounds;

        for (int i = 0; i <= rows; i++)
        {
            var start = new Vector3(bounds.min.x, 0, bounds.min.z + (bounds.size.z / rows) * i);
            var end = new Vector3(bounds.max.x, 0, bounds.min.z + (bounds.size.z / rows) * i);
            Gizmos.DrawLine(start, end);
        }

        for (int j = 0; j <= columns; j++)
        {
            var start = new Vector3(bounds.min.x + (bounds.size.x / columns) * j, 0, bounds.min.z);
            var end = new Vector3(bounds.min.x + (bounds.size.x / columns) * j, 0, bounds.max.z);
            Gizmos.DrawLine(start, end);
        }
    }
    
    private void AdjustObjectHeight(GameObject instance)
    {
        Transform baseTransform = instance.transform.Find("Base");

        if (baseTransform != null)
        {
            Vector3 offset = instance.transform.position - baseTransform.position;
            
            instance.transform.position = new Vector3(instance.transform.position.x, 0 + offset.y, instance.transform.position.z);
        }
        else
        {
            Debug.LogError("O GameObject 'Base' não foi encontrado no prefab " + instance.name);
        }
    }

    public List<GameObject> GetPrefabs()
    {
        List<GameObject> prefabs = new List<GameObject>();
        foreach (var obj in usedObjects)
        {
            prefabs.Add(obj.prefab);
        }
        return prefabs;
    }
}