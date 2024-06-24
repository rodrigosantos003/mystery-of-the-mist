using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    //Scripts
    private PlayerController playerController;
    
    //Variables
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private GameObject cameraSystem;
    [SerializeField] private GameObject player;
    
    //World Object Spawner Controller
    [SerializeField] private ObjectSpawner worldObjectsSpawner;
    [SerializeField] private float worldObjectsCellX;
    [SerializeField] private float worldObjectsCellY;
    
    //Enemy Spawner Controller
    [SerializeField] private ObjectSpawner enemiesSpawner;
    [SerializeField] private float enemiesObjectsCellX;
    [SerializeField] private float enemiesObjectsCellY;
    
    //Levels
    [SerializeField] private List<GameLevel> gameLevels;
    private int currentLevel = 0;
    
    [SerializeField] private SettingsMenu settingsMenu;
    
    [SerializeField] private GameOverScreen gameOverScreen;
    
    [SerializeField] private WinScreen winScreen;

    public int CurrentLevel
    {
        get => currentLevel;
        set => currentLevel = value;
    }
    
    //Enemy Controller
    [SerializeField] private Canvas healthBarCanvas;
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private ScriptableEnemies enemies;

    [SerializeField] private AnimationCurve distanceCurve;
    
    [SerializeField] private bool debugSpawner;
    [SerializeField] private bool debugHitDetection;

    [SerializeField] private List<PlayerAttack> availablePlayerAttacks;
    [SerializeField] private List<PlayerUpgrade> availablePlayerUpgrades;
    
    [SerializeField] private UIDocument upgradeCardsRoot;
    
    [SerializeField] private VisualTreeAsset playerAttackCardPrefab;
    [SerializeField] private VisualTreeAsset playerUpgradeCardPrefab;
    
    private VisualElement upgradeCardsContainer;

    [SerializeField] private float maxX;
    [SerializeField] private float maxZ;

    private static bool _isLoadedGame = false;
    
    [SerializeField]
    private GameState gameState;
    
    [SerializeField]
    private float intervalBetweenTurns;

    [SerializeField] private Canvas loadingScreen;
    
    [SerializeField] private GameObject textPopupPrefab;

    public static void SetIsLoadedGame(bool value)
    {
        _isLoadedGame = value;
    }

    private void Awake()
    {
        settingsMenu.LoadSettings();
    }

    private void Start()
    {
        loadingScreen.enabled = true;
        
        settingsMenu.LoadSettings();
        
        // Set the game state listener
        gameState.RegisterListener(HandleGameState);
        
        // Set the upgrade cards container
        upgradeCardsContainer = upgradeCardsRoot.rootVisualElement.Q<VisualElement>("UpgradeRoot");
        
        // Mostra o container de upgrades
        upgradeCardsContainer.AddToClassList("hidden");
        
        SetVariables();
        // Load the saved game or start a new one
        if (_isLoadedGame)
        {
            StartCoroutine(LoadSavedGame());
        }
        else
        {
            StartCoroutine(StartGame());
        }
    }

    /// <summary>
    /// Lógica do jogo
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartGame()
    {
        // Setup the level
        yield return StartCoroutine(SetupLevel(currentLevel));
    }

    /// <summary>
    /// Game turns
    /// </summary>
    /// <returns></returns>
    private IEnumerator TurnLogic()
    {
        // Tps the camera to the player position
        cameraSystem.transform.position = new Vector3(player.transform.position.x , cameraSystem.transform.position.y, player.transform.position.z);
        
        // Start the player turn
        yield return playerController.StartTurn();
        
        yield return new WaitForSeconds(intervalBetweenTurns);
        
        // Start the enemies turn
        yield return StartCoroutine(StartEnemiesTurn());

        // Repeat the process
        StartCoroutine(TurnLogic());
    }

    /// <summary>
    /// Set the scripts variables
    /// </summary>
    private void SetVariables()
    {
        playerController = player.GetComponent<PlayerController>();
        
        // Enemy Controller
        Enemy.SetHealthBarCanvas(healthBarCanvas);
        Enemy.SetHealthBarPrefab(healthBarPrefab);
        Enemy.SetEnemyScriptableObject(enemies);
        Enemy.SetPlayer(player);
        
        //World Object Spawner
        worldObjectsSpawner.SetCellSize(worldObjectsCellX, worldObjectsCellY);
        
        //Enemy Spawner
        enemiesSpawner.SetCellSize(enemiesObjectsCellX, enemiesObjectsCellY);
        
        //Player Controller
        playerController.SetGameState(gameState);
        
        //Calculate Hit Chance
        Utils.SetDistanceCurve(distanceCurve);
        
        //Debug
        Utils.SetDebug(debugHitDetection);
        ObjectSpawner.SetDebug(debugSpawner);
        
        //Set the max values
        CircleMesh.SetMaxValues(maxX, maxZ);
        
        //Set text popup prefab
        TextPopup.SetTextPopupPrefab(textPopupPrefab);
    }

    private IEnumerator SetupLevel(int level)
    {
        loadingScreen.enabled = true;
        
        gameState.CurrentState = GameState.State.Playing;
        
        //limpa o mapa
        foreach (Transform child in worldObjectsSpawner.transform)
        {
            Destroy(child.gameObject);
        }
        
        enemies.Clear();
        
        //spawna os objetos do level
        yield return StartCoroutine(worldObjectsSpawner.StartSpawning(gameLevels[level].WorldObjects));
        
        // Build the navmesh
        navMeshSurface.BuildNavMesh();
        
        // spawna os inimigos
        yield return StartCoroutine(enemiesSpawner.StartSpawning(gameLevels[level].Enemies));
        
        // Spawn the player
        yield return playerController.Spawn();

        playerController.Heal();
        
        
        // Espera 1 segundo para que a tela não pisque demasiado rápido
        yield return new WaitForSeconds(1);
        
        // Desativa a tela de loading
        loadingScreen.enabled = false;
        
        StartCoroutine(TurnLogic());
    }
    
    /// <summary>
    /// Start the enemies turn
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartEnemiesTurn()
    {
        // Check if the player won
        if (enemies.GetEnemies().Count == 0)
        {
            gameState.CurrentState = GameState.State.PlayerWon;
            yield break;
        }
        
        // Start each enemy turn
        foreach (var enemy in enemies.GetEnemies())
        {
            // Move the camera to the enemy position
            cameraSystem.transform.position = new Vector3(enemy.transform.position.x , cameraSystem.transform.position.y, enemy.transform.position.z);

            // Enemy movement and action
            yield return StartCoroutine(enemy.StartTurn());

            yield return new WaitForSeconds(intervalBetweenTurns);
        }
    }
    
    private void HandleGameState(GameState.State state)
    {
        switch (state)
        {
            case GameState.State.PlayerLost:
                StopAllCoroutines();
                Time.timeScale = 0;
                
                gameOverScreen.gameObject.SetActive(true);
                gameOverScreen.SetLevel(currentLevel + 1);
                break;
            case GameState.State.PlayerWon:
                StopAllCoroutines();
                
                if(currentLevel == gameLevels.Count - 1)
                {
                    Time.timeScale = 0;
                    winScreen.gameObject.SetActive(true);
                }
                
                StartCoroutine(ShowUpgrades());
                break;
        }
    }
    
    private IEnumerator ShowUpgrades()
    {
        yield return playerController.MoveToNextLevel();
        
        // Pausa o jogo
        Time.timeScale = 0;
        
        // Mostra o container de upgrades
        upgradeCardsContainer.RemoveFromClassList("hidden");
        
        upgradeCardsContainer.Clear();
        
        // Gera os cards de upgrade e adiciona ao container
        while(upgradeCardsContainer.childCount < 3)
        {
            var random = Random.Range(0, 2);
            
            var card = random == 0 ? GenerateAttackCard() : GenerateUpgradeCard();
            
            upgradeCardsContainer.Add(card);
        }
    }

    private VisualElement GenerateAttackCard()
    {
        //escolhe um ataque aleatorio
        var attack = availablePlayerAttacks[Random.Range(0, availablePlayerAttacks.Count)];
        
        //verifica se o jogador já tem esse ataque
        var playerAttack = playerController.Stats.Attacks.Find(x => x.AttackName == attack.AttackName);
        
        int nextLevel = playerAttack != null ? playerAttack.AttackLevel + 1: 1;

        var attackCard = attack.Card(playerAttackCardPrefab, nextLevel, playerController.Stats);
        
        if(attackCard == null)
        {
            return GenerateAttackCard();
        }
        
        //adiciona o evento de clique
        attackCard.RegisterCallback<ClickEvent>(e => HandleAttackCardClicked(attack));

        //retorna o card
        return attackCard;
    }
    
    private VisualElement GenerateUpgradeCard()
    {
        var upgradeChanceTotal = 0;
        
        foreach(var availableUpgrade in availablePlayerUpgrades)
        {
            upgradeChanceTotal += availableUpgrade.Chance;
        }
        
        
        var randomChance = Random.Range(0, upgradeChanceTotal);

        PlayerUpgrade upgrade = null;
        
        foreach(var chosenUpgrade in availablePlayerUpgrades)
        {
            randomChance -= chosenUpgrade.Chance;

            if (randomChance > 0) continue;
            
            upgrade = chosenUpgrade;
            break;
        }
        
        var upgradeCard = upgrade.Card(playerUpgradeCardPrefab);
        
        //adiciona o evento de clique
        upgradeCard.RegisterCallback<ClickEvent>(e => HandleUpgradeCardClicked(upgrade));

        //retorna o card
        return upgradeCard;
    }
    
    private void HandleAttackCardClicked(PlayerAttack attack)
    {
        var playerAttack = playerController.Stats.Attacks.Find(x => x.AttackName == attack.AttackName);
        if(playerAttack != null)
        {
            playerAttack.AttackLevel++;
        }
        else
        {
            playerController.Stats.Attacks.Add(attack);
        }
        
        Time.timeScale = 1;
        
        playerController.RefreshAttacks();
        upgradeCardsContainer.AddToClassList("hidden");

        StartCoroutine(SetupLevel(++currentLevel));
    }
    
    private void HandleUpgradeCardClicked(PlayerUpgrade upgrade)
    {
        switch (upgrade.Stat)
        {
            case PlayerUpgrade.Stats.Range:
                playerController.Stats.AttackRange = Mathf.CeilToInt(playerController.Stats.AttackRange * upgrade.Value);
                break;
            
            case PlayerUpgrade.Stats.Damage:
                playerController.Stats.Damage = Mathf.CeilToInt(playerController.Stats.Damage * upgrade.Value);
                break;
            
            case PlayerUpgrade.Stats.Health:
                playerController.Stats.MaxHealth = Mathf.CeilToInt(playerController.Stats.MaxHealth * upgrade.Value);
                break;
            
            case PlayerUpgrade.Stats.MovementRadius:
                playerController.Stats.MovementRange = Mathf.CeilToInt(playerController.Stats.MovementRange * upgrade.Value);
                break;
        }
        
        Time.timeScale = 1;
        
        playerController.RefreshAttacks();
        upgradeCardsContainer.AddToClassList("hidden");

        StartCoroutine(SetupLevel(++currentLevel));
    }

    #region Save and Load
    
    /// <summary>
    /// Loads the last saved game
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadSavedGame()
    {
        loadingScreen.enabled = true;
        // Set the game variables
        SetVariables();

        gameState.CurrentState = GameState.State.Playing;
        
        // Loads the objects to the world
        yield return StartCoroutine(LoadObjects());

        // Builds the navmesh
        navMeshSurface.BuildNavMesh();

        // Loads the enemies
        yield return StartCoroutine(LoadEnemies());

        // Loads the player
        yield return StartCoroutine(LoadPlayerData());
        
        // Loads the level
        yield return StartCoroutine(LoadLevelData());

        yield return new WaitForSeconds(1);
        
        // Desativa a tela de loading
        loadingScreen.enabled = false;
        
        StartCoroutine(TurnLogic());
    }
    
    /// <summary>
    /// Loads saved enemies
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadEnemies()
    {
        EnemyDataWrapper objectData = SaveSystem.LoadEnemies();
        
        if (objectData != null)
        {
            foreach (var enemy in objectData.enemies)
            {
                var prefab = Resources.Load<GameObject>("Prefabs/Enemies/" + enemy.type);
                
                Vector3 objPosition = new Vector3(enemy.position.X, enemy.position.Y, enemy.position.Z);
                Vector3 objRotation = new Vector3(enemy.rotation.X, enemy.rotation.Y, enemy.rotation.Z);
                
                var enemyInstance = Instantiate(prefab, objPosition, Quaternion.Euler(objRotation));
                
                var enemyController = enemyInstance.GetComponent<Enemy>();
                
                enemyInstance.transform.parent = enemiesSpawner.transform;
                
                enemyController.SetCurrentHealth(enemy.health);
            }
        }
        
        yield return true;
    }

    /// <summary>
    /// Loads saved world objects
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadObjects()
    {
        WorldObjectDataWrapper objectData = SaveSystem.LoadWorldObjects();
        
        if (objectData != null)
        {
            foreach (var obj in objectData.worldObjects)
            {
                var prefab = Resources.Load<GameObject>("Prefabs/WorldObjects/" + obj.type);
                
                Vector3 objPosition = new Vector3(obj.position.X, obj.position.Y, obj.position.Z);
                Vector3 objRotation = new Vector3(obj.rotation.X, obj.rotation.Y, obj.rotation.Z);
                
                var instance = Instantiate(prefab, objPosition, Quaternion.Euler(objRotation));
                instance.transform.parent = worldObjectsSpawner.transform;
            }
        }
        
        yield return true;
    }
    
    public IEnumerator LoadPlayerData()
    {
        PlayerData playerData = SaveSystem.LoadPlayer();
        if (playerData != null)
        {
            Vector3 playerPosition = new Vector3(playerData.Position.x, playerData.Position.y, playerData.Position.z);
            Quaternion playerRotation = new Quaternion(playerData.Rotation.X, playerData.Rotation.Y, playerData.Rotation.Z, playerData.Rotation.W);
            
            playerController.Stats.Attacks.Clear();
            
            foreach (var savedAttack in playerData.Attacks)
            {
                var attack = availablePlayerAttacks.Find(x => x.AttackName == savedAttack.AttackName);
                attack.AttackLevel = savedAttack.AttackLevel;
                playerController.Stats.Attacks.Add(attack);
            }
            
            playerController.gameObject.SetActive(true);
            playerController.transform.position = playerPosition;
            playerController.transform.rotation = playerRotation;
            
            playerController.Stats.MaxHealth = playerData.MaxHealth;
            playerController.Stats.Damage = playerData.Damage;
            playerController.Stats.MovementRange = playerData.MovementRange;
            playerController.Stats.AttackRange = playerData.AttackRange;
            playerController.CurrentHealth = playerData.CurrentHealth;
            
            playerController.RefreshAttacks();
            
            playerController.RefreshHealth();
            
            yield return true;
        }
    }
    
    public IEnumerator LoadLevelData()
    {
        var levelData = SaveSystem.LoadLevel();
        
        currentLevel = levelData;
        
        yield return true;
    }
    #endregion
}
