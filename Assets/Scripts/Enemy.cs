using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    private static GameObject _healthBarPrefab;
    
    private static Canvas _healthBarCanvas;

    private static GameObject _player;
    
    [SerializeField] private ScriptableEnemyStats stats;
    private float _currentHealth;
    
    private static ScriptableEnemies _enemies;
    
    private ProgressBar _healthBar;
    
    private GameObject _healthBarObject;
    
    private Camera _mainCamera;

    private float _enemyHeight;

    private NavMeshAgent _agent;
    
    [SerializeField]
    private CircleMesh moveCircleMesh;
    
    [SerializeField]
    private CircleMesh attackCircleMesh;
    
    private const float MinimumVisibility = 0.40f;

    private Animator _animator;

    private bool isShowingMeshes;

    public float GetCurrentHealth()
    {
        return _currentHealth;
    }


    #region Setters

    public void SetCurrentHealth(float value)
    {
        _currentHealth = value;
    }

    public static void SetHealthBarCanvas(Canvas canvas)
    {
        _healthBarCanvas = canvas;
    }
    
    public static void SetHealthBarPrefab(GameObject prefab)
    {
        _healthBarPrefab = prefab;
    }
    
    public static void SetEnemyScriptableObject(ScriptableEnemies enemies)
    {
        _enemies = enemies;
    }
    
    public static void SetPlayer(GameObject player)
    {
        _player = player;
    }
    
    #endregion
    
    private void Start()
    {
        _enemies.AddEnemy(this);
        
        _mainCamera = Camera.main;
        _enemyHeight = GetComponent<Collider>().bounds.size.y + 0.5f;
        _agent = GetComponent<NavMeshAgent>();
        _agent.enabled = true;
        _animator = GetComponent<Animator>();

        _currentHealth = stats.MaxHealth;
        
        SpawnHealthBar();
        
        moveCircleMesh.SetCircleRadius(stats.MoveRadius);
        attackCircleMesh.SetCircleRadius(stats.AttackRange);
    }

    private void LateUpdate()
    {
        HealthbarLookAtCamera();
    }
    
    public IEnumerator StartTurn()
    {
        var currentPosition = transform.position;
        currentPosition.y = 1;
        
        //Move-se para perto do player
        if (Vector3.Distance(currentPosition, _player.transform.position) > stats.AttackRange)
        {
            _animator.SetBool("isMoving", true);
            yield return MoveTowardsPlayer();
            _animator.SetBool("isMoving", false);
        }
        else
        {
            float currentVisibility = Utils.CalculateVisibility(transform.position, _player, stats.AttackBounds).Visibility;
            
            if(currentVisibility < MinimumVisibility)
            {
                _animator.SetBool("isMoving", true);
                yield return MoveWithinRange();
                _animator.SetBool("isMoving", false);
            }
        }

        yield return Attack();
    }
    

    public IEnumerator MoveTowardsPlayer()
    {
        var finalPosition = GetPositionTowardsPlayer();
        
        _agent.SetDestination(finalPosition);
        
        yield return ReachedDestination();
    }
    
    public IEnumerator MoveWithinRange()
    {
        var finalPosition = GetPositionWithinRange();
        
        _agent.SetDestination(finalPosition);
        
        yield return ReachedDestination();
    }
    
    private Vector3 GetPositionTowardsPlayer()
    {
        var playerPosition = _player.transform.position;
        
        var angle = Random.Range(-30, 30);
        
        var direction = playerPosition - transform.position;
        
        direction = Quaternion.Euler(0, angle, 0) * direction;
        
        var newPosition = transform.position + direction.normalized * stats.MoveRadius;
        
        newPosition.y = 0;

        return newPosition;
    }

    private Vector3 GetPositionWithinRange()
    {
        float minimumVisibility = MinimumVisibility;
        float tries = 0;
        Vector3 newPosition = Vector3.zero;
        
        while (true)
        {
            var playerPosition = _player.transform.position;

            playerPosition.y = transform.position.y;
        
            // Gerar um ponto aleatório dentro de um círculo unitário
            Vector2 randomPoint = Random.insideUnitCircle;
        
            // Converter o ponto aleatório para um ponto no círculo de raio distance
            Vector3 randomDirection = new Vector3(randomPoint.x, 0, randomPoint.y);
            
            float minDistance = 0.4f * stats.AttackRange;
            float maxDistance = 0.7f * stats.AttackRange;
            float distance = Random.Range(minDistance, maxDistance);
            
            newPosition = playerPosition + randomDirection.normalized * distance;

            // Reduzir a visibilidade mínima a cada 10 tentativas
            if (tries++ % 5 == 0) minimumVisibility *= 0.9f;

            if (tries > 1000)
            {
                newPosition = transform.position + randomDirection.normalized * stats.MoveRadius;
                break;
            }
            // Verificar se a nova posição é válida na NavMesh
            if (!NavMesh.SamplePosition(newPosition, out _, 1f, NavMesh.AllAreas))
            {
                continue;
            }

            // Verificar se a nova posição está dentro do raio de movimento
            if (Vector3.Distance(transform.position, newPosition) > stats.MoveRadius)
            {
                continue;
            }

            // Verificar a visibilidade da nova posição
            if (Utils.CalculateVisibility(newPosition, _player, stats.AttackBounds).Visibility < minimumVisibility)
            {
                continue;
            }

            // Se todas as condições forem atendidas, sair do loop
            break;
        }

        return newPosition;
    }

    
    
    private IEnumerator ReachedDestination()
    {
        yield return new WaitUntil(() =>
        {
            KeepHealthBarAboveEnemy();
            
            if (_agent.destination == null) return false;
            
            Vector3 currentPosition = transform.position;

            Vector3 finalDestination = _agent.destination;
            
            currentPosition.y = 0;
            finalDestination.y = 0;

            bool isWithinMargin = Vector3.Distance(currentPosition, finalDestination) < 0.1;
            bool isMoving = _agent.velocity.sqrMagnitude > 0.01f;

            if (isWithinMargin && !isMoving)
            {
                _agent.ResetPath();
                return true;
            }

            return false;
        });
    }
    
    #region HealthBar
    private void SpawnHealthBar()
    {
        var healthBarPosition = transform.position + new Vector3(0, _enemyHeight, 0);
        _healthBarObject = Instantiate(_healthBarPrefab, healthBarPosition, Quaternion.identity, _healthBarCanvas.transform);
        _healthBar = _healthBarObject.GetComponent<ProgressBar>();
        _healthBar.SetProgress(_currentHealth / stats.MaxHealth, 2);
    }
    
    public void HealthbarLookAtCamera()
    {
        _healthBar.gameObject.transform.rotation = Quaternion.LookRotation(_healthBar.gameObject.transform.position - Camera.main.transform.position);
    }
    
    private void KeepHealthBarAboveEnemy()
    {
        _healthBarObject.transform.position = transform.position + new Vector3(0, _enemyHeight, 0);
    }
    #endregion
    public void OnTakeDamage(int damage)
    {
        _currentHealth -= damage;

        _healthBar.SetProgress(_currentHealth / stats.MaxHealth, 2);
        
        if (_currentHealth <= 0)
        {
            Destroy(gameObject);
            Destroy(_healthBar.gameObject);
            _enemies.DeselectEnemy();
            _enemies.RemoveEnemy(this);
        }
    }

    private void OnMouseDown()
    {
        if (_enemies.GetSelectedEnemy() == this)
        {
            _enemies.DeselectEnemy();
        }
        else
        {
            _enemies.SetSelectedEnemy(this);
        }
    }

    private void OnMouseOver()
    {
        if (!isShowingMeshes)
        {
            attackCircleMesh.DrawMesh();
            moveCircleMesh.DrawMesh();
            isShowingMeshes = true;
        }
    }
    
    private void OnMouseExit()
    {
        attackCircleMesh.HideMesh();
        moveCircleMesh.HideMesh();
        isShowingMeshes = false;
    }

    public IEnumerator Attack()
    {
        if (Vector3.Distance(transform.position, _player.transform.position) > stats.AttackRange)
            yield break;
        
        var hitChance = Utils.GetHitChance(gameObject, _player, stats.AttackRange, stats.AttackBounds);
        
        if (Random.Range(0, 100) > hitChance.Chance)
        {
            TextPopup.Create(transform.position + new Vector3(0, _enemyHeight, 0), "Miss! (" + hitChance.Chance + "%)", Color.red, 10);
            yield return new WaitForSeconds(2f);
        }
        else
        {
            TextPopup.Create(transform.position + new Vector3(0, _enemyHeight, 0), "Hit! (" + hitChance.Chance + "%)", Color.green, 10);

            var attackPosition = hitChance.SucessfulHits[Random.Range(0, hitChance.SucessfulHits.Count)].point;
            
            Debug.DrawLine(transform.position, attackPosition, Color.green, 5f);
            
            transform.LookAt(new Vector3(_player.transform.position.x, transform.position.y, _player.transform.position.z));
            
            _animator.SetTrigger("Attack");
            yield return new WaitForSeconds(1.2f);
            
            var projectile = stats.ExecuteAttack(attackPosition, gameObject);
            
            yield return new WaitUntil(() => projectile == null);
        }
    }
}