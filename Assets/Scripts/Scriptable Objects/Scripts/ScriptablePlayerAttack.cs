using System;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "PlayerAttack", menuName = "ScriptableObjects/Player Attack")]
public class PlayerAttack : ScriptableObject
{
    [SerializeField] private GameObject _attackPrefab;
    
    [SerializeField] private string _attackName;
    [SerializeField] private string _attackDescription;
    [SerializeField] private Texture2D _attackImage;
    [SerializeField] private int _defaultAttackLevel;
    [SerializeField] private int _attackLevel;
    
    public string AttackName { get => _attackName; }
    public string AttackDescription { get => _attackDescription; }
    public Texture2D AttackImage { get => _attackImage; }
    public ScriptableAttackInfo AttackInfo => _attackPrefab.GetComponent<ProjectileAttack>().AttackInfo;
    public Bounds AttackBounds => Utils.GetBoundsBeforeInstantiate(_attackPrefab);
    public GameObject AttackPrefab { get => _attackPrefab; }
    
    private void OnEnable()
    {
        _attackLevel = _defaultAttackLevel;
    }
    
    public int AttackLevel
    {
        get => _attackLevel;
        set => _attackLevel = value;
    }

    public VisualElement Card (VisualTreeAsset template, int level, ScriptablePlayerStats stats)
    {
        //verifica se o level é válido
        if (level > AttackInfo.AttackUpgrade.Length)
        {
            return null;
        }
        
        //clona o template do card
        var attackCard = template.CloneTree();
        
        //preenche o card com as informações do ataque
        attackCard.Q<Label>("Title").text = AttackName + " (lvl " + level + ")";
        attackCard.Q<Label>("Image").style.backgroundImage = AttackImage;
        attackCard.Q<Label>("Description").text = AttackDescription;
        
        attackCard.Q<Label>("Attack").text = AttackInfo.GetDamage(stats.Damage, level).ToString();
        attackCard.Q<Label>("Range").text = AttackInfo.GetRange(stats.AttackRange, level).ToString();
        
        attackCard.Q<Label>("AttackInfo").text = "(base * " + AttackInfo.GetDamageMultipler(level) + ")";
        attackCard.Q<Label>("RangeInfo").text = "(base * " + AttackInfo.GetRangeMultipler(level) + ")";
        
        //retorna o card
        return attackCard;
    }
}