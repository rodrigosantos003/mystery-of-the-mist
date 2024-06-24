using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "ScriptableObjects/SelectedEnemy")]
public class ScriptableEnemies : ScriptableObject
{
    private List<Enemy> _enemies = new List<Enemy>();
    [SerializeField] 
    int selectedEnemyIndex = -1;
    public delegate void OnValueChanged();
    [SerializeField] 
    private List<OnValueChanged> _listeners = new List<OnValueChanged>();
    
    public void AddListener (OnValueChanged listener)
    {
        _listeners.Add(listener);
    }
    
    public void RemoveListener(OnValueChanged listener)
    {
        _listeners.Remove(listener);
    }
    
    private void CallDelegates()
    {
        foreach (var listener in _listeners)
        {
            listener.Invoke();
        }
    }
    
    public void Clear()
    {
        selectedEnemyIndex = -1;
        CallDelegates();
        _enemies.Clear();
    }
    
    void OnEnable()
    {
        _enemies.Clear();
        selectedEnemyIndex = -1;
    }
    public void AddEnemy(Enemy enemy)
    {
        _enemies.Add(enemy);
    }
    
    public void RemoveEnemy(Enemy enemy)
    {
        _enemies.Remove(enemy);
    }
    
    public List<Enemy> GetEnemies()
    {
        return _enemies;
    }
    
    public Enemy GetSelectedEnemy()
    {
        if(_enemies.Count == 0 || selectedEnemyIndex == -1)
        {
            return null;
        }
        
        return _enemies[selectedEnemyIndex];
    }
    
    public void SetSelectedEnemy(Enemy enemy)
    {
        selectedEnemyIndex = _enemies.IndexOf(enemy);
        CallDelegates();
    }
    
    public void DeselectEnemy()
    {
        selectedEnemyIndex = -1;
        CallDelegates();
    }
}
