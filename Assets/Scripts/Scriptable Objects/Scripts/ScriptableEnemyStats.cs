using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "ScriptableObjects/Enemy Stats")]
public class ScriptableEnemyStats : ScriptableObject
{
    //Stats do inimigo (em valores inteiros)
    [SerializeField] private int _moveRadius;
    [SerializeField] private int _maxHealth;
    [SerializeField] private int _attackRange;
    [SerializeField] private int _attackDamage;

    //Prefab do ataque do inimigo (só tem 1 ataque)
    [SerializeField] private GameObject _attackPrefab;
    
    public Bounds AttackBounds => Utils.GetBoundsBeforeInstantiate(_attackPrefab);
    
    public int MoveRadius
    {
        get => _moveRadius;
        set => _moveRadius = value;
    }
    
    public int MaxHealth
    {
        get => _maxHealth;
        set => _maxHealth = value;
    }
    
    public int AttackRange
    {
        get => _attackRange;
        set => _attackRange = value;
    }
    
    public int AttackDamage
    {
        get => _attackDamage;
        set => _attackDamage = value;
    }

    public GameObject ExecuteAttack(Vector3 targetPosition, GameObject source)
    {
        var sourcePosition = source.transform.position;
        
        var direction = (targetPosition - sourcePosition).normalized;
    
        var rotation = Quaternion.LookRotation(direction);
    
        var sourceBounds = source.GetComponent<Collider>().bounds;
        var attackBounds = AttackBounds;
    
        var spawnPosition = sourcePosition + direction * (sourceBounds.extents.magnitude + attackBounds.extents.magnitude);

        var attack = Instantiate(_attackPrefab, spawnPosition, rotation).GetComponent<ProjectileAttack>();
    
        
        var attackInfo = attack.AttackInfo;

        attack.SetAttackDamage(attackInfo.GetDamage(AttackDamage));
        
        attack.PlaySoundEffect();

        return attack.gameObject;
    }
}
