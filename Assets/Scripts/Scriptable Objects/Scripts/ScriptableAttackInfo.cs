using UnityEngine;

[CreateAssetMenu(fileName = "AttackInfo", menuName = "ScriptableObjects/Attack Info")]
public class ScriptableAttackInfo : ScriptableObject
{
    [SerializeField] private float projectileSpeed;
    [SerializeField] private AttackUpgrade[] attackUpgrade;
    [SerializeField] private AudioClip attackSound;
    
    public float ProjectileSpeed => projectileSpeed;
    
    public AudioClip AttackSound => attackSound;
    
    public AttackUpgrade[] AttackUpgrade => attackUpgrade;
    
    public float GetDamageMultipler(int _level = 1)
    {
        return attackUpgrade[_level-1].AttackMultiplier;
    }
    
    public float GetRangeMultipler(int _level = 1)
    {
        return attackUpgrade[_level-1].RangeMultiplier;
    }
    
    public int GetDamage(int damage, int _level = 1)
    {
        return (int)(attackUpgrade[_level-1].AttackMultiplier * damage);
    }
    
    public int GetRange(int attackRange, int _level = 1)
    {
        return (int)(attackUpgrade[_level-1].RangeMultiplier * attackRange);
    } 
}
