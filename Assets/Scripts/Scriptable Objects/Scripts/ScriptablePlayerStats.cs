using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttack", menuName = "ScriptableObjects/Player Stats")]
public class ScriptablePlayerStats : ScriptableObject
{
    //Variaveis default
    [SerializeField] private int _defaultMaxHealth;
    [SerializeField] private int _defaultDamage;
    [SerializeField] private int _defaultMovementRange;
    [SerializeField] private int _defaultAttackRange;
    
    //Variaveis do jogador
    public int MaxHealth { get; set; }
    public int Damage { get; set; }
    public int MovementRange { get; set; }
    public int AttackRange { get; set; }
    
    //Lista de ataques
    [SerializeField] private List<PlayerAttack> _defaultAttacks;
    
    private List<PlayerAttack> _attacks;
    
    public List<PlayerAttack> Attacks => _attacks;
    
    //No enable seta as variaveis default
    private void OnEnable()
    {
        MaxHealth = _defaultMaxHealth;
        Damage = _defaultDamage;
        MovementRange = _defaultMovementRange;
        AttackRange = _defaultAttackRange;
        _attacks = new List<PlayerAttack>(_defaultAttacks);
    }
    
    //Função de ataque
    public GameObject ExecuteAttack(Vector3 targetPosition, GameObject source, PlayerAttack playerAttack)
    {
        var sourcePosition = source.transform.position;
        
        var direction = (targetPosition - sourcePosition).normalized;
    
        var rotation = Quaternion.LookRotation(direction);
    
        var sourceBounds = source.GetComponent<Collider>().bounds;
        var attackBounds = playerAttack.AttackBounds;
    
        var spawnPosition = sourcePosition + direction * (sourceBounds.extents.magnitude + attackBounds.extents.magnitude);

        var attack = Instantiate(playerAttack.AttackPrefab, spawnPosition, rotation).GetComponent<ProjectileAttack>();
    
        var attackInfo = attack.AttackInfo;
    
        attack.SetAttackDamage(Damage);
        attack.SetAttackLevel(playerAttack.AttackLevel);
        
        attack.PlaySoundEffect();
        
        return attack.gameObject;
    }
}
