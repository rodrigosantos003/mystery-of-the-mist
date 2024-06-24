using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameLevel", menuName = "ScriptableObjects/Game Level")]
public class GameLevel : ScriptableObject
{
    [SerializeField] private List<ObjectInWorld> _worldObjects;
    [SerializeField] private List<ObjectInWorld> _enemies;
    
    public List<ObjectInWorld> WorldObjects => _worldObjects;
    public List<ObjectInWorld> Enemies => _enemies;
}
