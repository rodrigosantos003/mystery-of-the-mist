using UnityEngine;

public class ProjectileAttack : MonoBehaviour
{
    private float _attackDamage = 0f;
    
    [SerializeField]
    private ScriptableAttackInfo _attackInfo;

    private int _attackLevel = 1;

    public void SetAttackDamage(float damage)
    {
        _attackDamage = damage;
    }
    
    public void SetAttackLevel(int level)
    {
        _attackLevel = level;
    }
    
    public void PlaySoundEffect()
    {
        if (_attackInfo.AttackSound != null)
        {
            Camera.main.GetComponent<AudioSource>().PlayOneShot(_attackInfo.AttackSound);
        }
    }
    
    public ScriptableAttackInfo AttackInfo => _attackInfo;

    private void FixedUpdate()
    {
        transform.position += transform.forward * (_attackInfo.ProjectileSpeed * Time.deltaTime);
    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Vector3 topPosition = other.bounds.center + new Vector3(0, other.bounds.extents.y, 0);
            
            TextPopup.Create(topPosition, AttackInfo.GetDamage((int)_attackDamage, _attackLevel).ToString(), new Color(255, 43, 0), 7);
            
            other.GetComponent<Enemy>().OnTakeDamage(AttackInfo.GetDamage((int)_attackDamage, _attackLevel));
            
            
            Destroy(gameObject);
        }
        else if (other.CompareTag("Player"))
        {
            Vector3 topPosition = other.bounds.center + new Vector3(0, other.bounds.extents.y, 0);
            
            TextPopup.Create(topPosition, AttackInfo.GetDamage((int)_attackDamage).ToString(), new Color(255, 43, 0), 7);
            
            other.GetComponent<PlayerController>().OnTakeDamage(AttackInfo.GetDamage((int)_attackDamage, _attackLevel));
            Destroy(gameObject);
        }
        else if (other.CompareTag("Object"))
        {
            Destroy(gameObject);
        }
    }
}
