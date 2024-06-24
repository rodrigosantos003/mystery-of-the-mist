using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    private float _moveRadius;
    
    private NavMeshAgent _agent;

    [SerializeField]
    private BoxCollider _spawnArea;
    
    [SerializeField]
    private CircleMesh _moveCircle;
    
    [SerializeField]
    private ScriptablePlayerStats _stats;
    private float _currentHealth;

    [SerializeField] private GameObject levelEnd;
    
    public float CurrentHealth {
        get => _currentHealth;
        set => _currentHealth = value;
    }
    
    public ScriptablePlayerStats Stats => _stats;
    
    private PlayerAttack _currentAttack;
    
    [SerializeField]
    private ScriptableEnemies _enemies;
    
    private Animator _animator;
    
    private GameState _gameState;

    [SerializeField] private CircleMesh _attackCircle;

    [SerializeField] private UIDocument _playerUI;
    [SerializeField] private VisualTreeAsset _attackCardTemplate;
    private VisualElement attackContainer;
    private Button attackButton;

    private Utils.HitChance hitChance;
    private Label hitChanceLabel;
    
    private bool waitingForAttack = false;
    private bool wantsToAttack = false;
    
    [SerializeField] private ProgressBar _healthBar;
    [SerializeField] private TextMeshProUGUI _healthText;
    
    private void Start()
    {
        gameObject.SetActive(false);
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _agent.enabled = true;
        _moveCircle.SetOnMouseDownAction(MoveToDestination);
        _enemies.AddListener(CalculateHitChance);
        _currentHealth = _stats.MaxHealth;
        
        attackContainer = _playerUI.rootVisualElement.Q<VisualElement>("PlayerAttacks");
        hitChanceLabel = _playerUI.rootVisualElement.Q<Label>("AttackChance");
        attackButton = _playerUI.rootVisualElement.Q<Button>("AttackButton");

        hitChanceLabel.text = "";
        attackButton.visible = false;
        attackContainer.visible = false;
        
        _healthBar.SetProgress(_currentHealth / _stats.MaxHealth);
        _healthText.text = _currentHealth.ToString();
        
        attackButton.clicked += () =>
        {
            if (waitingForAttack) wantsToAttack = true;
        };
        
        RefreshAttacks();
    }

    public void RefreshAttacks()
    {
        attackContainer.Clear();
        foreach(PlayerAttack attack in _stats.Attacks)
        {
            var card = attack.Card(_attackCardTemplate, attack.AttackLevel, _stats);
            if (card != null)
            {
                attackContainer.Add(card);
                card.RegisterCallback<MouseUpEvent>(evt => HandleAttackClicked(attack, card));
            }
        }
    }
    
    private void HandleAttackClicked(PlayerAttack attack, VisualElement card)
    {
        if(!waitingForAttack) return;
        
        if (_currentAttack == attack)
        {
            _currentAttack = null;
            card.RemoveFromClassList("selectedCard");
            
            _attackCircle.HideMesh();
        }
        else
        {
            _currentAttack = attack;
        
            foreach (var child in attackContainer.Children())
            {
                child.RemoveFromClassList("selectedCard");
            }
        
            card.AddToClassList("selectedCard");
            
            _attackCircle.SetCircleRadius(attack.AttackInfo.GetRange(_stats.AttackRange));
            _attackCircle.DrawMesh();
        }
        CalculateHitChance();
    }
    
    public void SetGameState(GameState gameState)
    {
        _gameState = gameState;
    }
    
    public void OnTakeDamage(int damage)
    {
        _currentHealth -= damage;
        _healthBar.SetProgress(_currentHealth / _stats.MaxHealth);
        _healthText.text = _currentHealth.ToString();
        if (_currentHealth <= 0)
        {
            _gameState.CurrentState = GameState.State.PlayerLost;
            gameObject.SetActive(false);
        }
    }

    public bool CheckIfAreEnemiesInRange()
    {
        var enemies = _enemies.GetEnemies();
        var attacks = _stats.Attacks;
        
        foreach (var enemy in enemies)
        {
            foreach (var attack in attacks)
            {
                var attackRange = attack.AttackInfo.GetRange(_stats.AttackRange);
                
                if(Vector3.Distance(enemy.transform.position, transform.position) > attackRange) continue;
                
                return true;
            }
        }

        return false;
    }

    private void CalculateHitChance()
    {
        var enemy = _enemies.GetSelectedEnemy();
        var attack = _currentAttack;
        
        if (enemy == null || attack == null)
        {
            hitChance = new Utils.HitChance();
            hitChanceLabel.text = "";
            attackButton.visible = false;
            return;
        }
        
        hitChance = Utils.GetHitChance(gameObject, enemy.gameObject, attack.AttackInfo.GetRange(_stats.AttackRange),
            attack.AttackBounds);
        
        hitChanceLabel.text = hitChance.Chance + "%";
        attackButton.visible = true;
    }

    public void Heal()
    {
        _currentHealth = _stats.MaxHealth;
        _healthText.text = _currentHealth.ToString();
        _healthBar.SetProgress(1);
    }
    public IEnumerator Attack()
    {
        attackContainer.visible = true;
        yield return new WaitUntil(() => wantsToAttack);

        attackContainer.visible = false;
        var enemy = _enemies.GetSelectedEnemy();
        var attack = _currentAttack;
        
        _attackCircle.HideMesh();
        wantsToAttack = false;
        waitingForAttack = false;
        hitChanceLabel.text = "";
        attackButton.visible = false;
        
        
        if (Random.Range(0, 100) > hitChance.Chance)
        {
            TextPopup.Create(transform.position, "Miss! (" + hitChance.Chance + "%)", Color.red, 10);
            yield return new WaitForSeconds(2f);
        }
        else
        {
            TextPopup.Create(transform.position, "Hit! (" + hitChance.Chance + "%)", Color.green, 10);
            var attackPosition = hitChance.SucessfulHits[Random.Range(0, hitChance.SucessfulHits.Count)].point;
            
            Debug.DrawLine(transform.position, attackPosition, Color.green, 5f);
            
            transform.LookAt(new Vector3(enemy.transform.position.x, transform.position.y, enemy.transform.position.z));
            
            _animator.SetTrigger("Attack");
            yield return new WaitForSeconds(1.2f);
            var projectile = _stats.ExecuteAttack(attackPosition, gameObject, attack);
            
            yield return new WaitUntil(() => projectile == null);
        }
        
        _enemies.DeselectEnemy();
        _currentAttack = null;
    }

    public IEnumerator StartTurn()
    {
        _moveCircle.SetCircleRadius(_stats.MovementRange);
        yield return StartMoving();
        waitingForAttack = true;
        if (CheckIfAreEnemiesInRange()) yield return Attack();
        waitingForAttack = false;
    }
    
    public void RefreshHealth()
    {
        _healthText.text = _currentHealth.ToString();
        _healthBar.SetProgress(_currentHealth / _stats.MaxHealth);
    }
    
    public IEnumerator StartMoving()
    {
        _moveCircle.DrawMesh();
        
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        
        yield return new WaitUntil(() => _agent.hasPath);
        
        _animator.SetBool("isMoving", true);
        yield return ReachedDestination();
        _animator.SetBool("isMoving", false);
    }
    
    public IEnumerator ReachedDestination()
    {
        yield return new WaitUntil(() =>
        {
            if (_agent.destination == null) return false;
            
            Vector3 currentPosition = transform.position;

            Vector3 finalDestination = _agent.destination;
            
            currentPosition.y = 0;
            finalDestination.y = 0;

            bool isWithinMargin = Vector3.Distance(currentPosition, finalDestination) < 1;
            bool isMoving = _agent.velocity.sqrMagnitude > 0.01f;
            
            if(isWithinMargin && !isMoving) _agent.ResetPath();
            
            return isWithinMargin && !isMoving;
        });
    }
    
    private void MoveToDestination(Vector3 destination)
    {
        if(_gameState.CurrentState == GameState.State.Paused) return;
        _agent.SetDestination(destination);
        _moveCircle.HideMesh();
    }

    #region PlayerSpawner
    public IEnumerator Spawn()
    {
        Vector3 spawnPosition;
        var bounds = _spawnArea.bounds;

        var attempts = 0;
        do
        {
            var x = Random.Range(bounds.min.x, bounds.max.x);
            var z = Random.Range(bounds.min.z, bounds.max.z);
            var y = 1;
        
            spawnPosition = new Vector3(x, y, z);
        
            if (attempts++ > 100)
            {
                Debug.LogError("Não foi encontrado um local para spawnar o jogador");
                yield return null;
            }
        }
        while (IsPositionAvailable(spawnPosition, gameObject));
        
        
        transform.position = spawnPosition;
        
        gameObject.SetActive(true);
        
        yield return true;
    }

    private bool IsPositionAvailable(Vector3 position, GameObject obj)
    {
        var radius = obj.GetComponent<Collider>().bounds.extents.magnitude + 5;
        return Physics.CheckSphere(position, radius, LayerMask.GetMask("Object"));
    }

    public IEnumerator MoveToNextLevel()
    {
        _agent.speed = 8f;
        _animator.SetBool("isMoving", true);
        MoveToDestination(levelEnd.transform.position);
        
        yield return ReachedDestination();
        _animator.SetBool("isMoving", false);
        _agent.speed = 5f;
    }
    #endregion
}