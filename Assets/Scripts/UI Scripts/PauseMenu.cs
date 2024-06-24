using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenu : MonoBehaviour
{
    private UIDocument _pauseMenu;

    private Button _resumeButton;
    private Button _saveButton;
    private Button _settingsButton;
    private Button _quitButton;

    private bool _isPaused = false;
    
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private UIDocument _settingsMenu;
    
    [SerializeField] private GameState _gameState;

    // Start is called before the first frame update
    void Start()
    {
        _pauseMenu = GetComponent<UIDocument>();
        _pauseMenu.rootVisualElement.style.display = DisplayStyle.None;

        _resumeButton = _pauseMenu.rootVisualElement.Q<Button>("ResumeButton");
        _saveButton = _pauseMenu.rootVisualElement.Q<Button>("SaveButton");
        _settingsButton = _pauseMenu.rootVisualElement.Q<Button>("SettingsButton");
        _quitButton = _pauseMenu.rootVisualElement.Q<Button>("QuitButton");

        _resumeButton.RegisterCallback<ClickEvent>(ResumeGame);
        _saveButton.RegisterCallback<ClickEvent>(SaveGame);
        _settingsButton.RegisterCallback<ClickEvent>(OpenSettings);
        _quitButton.RegisterCallback<ClickEvent>(QuitGame);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused)
            {
                _gameState.CurrentState = GameState.State.Playing;
                Time.timeScale = 1;
                _pauseMenu.rootVisualElement.style.display = DisplayStyle.None;
            }
            else
            {
                _gameState.CurrentState = GameState.State.Paused;
                Time.timeScale = 0;
                _pauseMenu.rootVisualElement.style.display = DisplayStyle.Flex;
                
                _saveButton.text = "Save Game";
                _saveButton.style.color = Color.black;
                _saveButton.style.backgroundColor = Color.white;
            }

            _isPaused = !_isPaused;
        }
    }

    void ResumeGame(ClickEvent ev)
    {
        Time.timeScale = 1;
        _pauseMenu.rootVisualElement.style.display = DisplayStyle.None;
    }

    void SaveGame(ClickEvent ev)
    {
        if(_saveButton.text == "Game Saved") return;
        
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        List<GameObject> enemies = new List<GameObject>();
        foreach (Transform child in GameObject.Find("Enemies").transform)
        {
            enemies.Add(child.gameObject);
        }

        List<GameObject> worldObjects = new List<GameObject>();
        foreach (Transform child in GameObject.Find("WorldObjects").transform)
        {
            worldObjects.Add(child.gameObject);
        }
        
        SaveSystem.Save(player, enemies.ToArray(), worldObjects.ToArray(), _gameManager.CurrentLevel);

        _saveButton.text = "Game Saved";
        _saveButton.style.color = Color.white;
        _saveButton.style.backgroundColor = Color.green;
    }

    void OpenSettings(ClickEvent ev)
    {
        _settingsMenu.gameObject.SetActive(true);
    }

    void QuitGame(ClickEvent ev)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }
}
