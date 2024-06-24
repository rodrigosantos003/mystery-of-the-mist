using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    private UIDocument _document;
    private Button _startButton;
    private Button _loadButton;
    private Button _settingsButton;
    private Button _quitButton;
    
    [SerializeField] private SettingsMenu _settingsMenu;

    private void Awake()
    {
        _settingsMenu.LoadSettings();
        
        // Get buttons by name
        _document = GetComponent<UIDocument>();
        _startButton = _document.rootVisualElement.Q<Button>("StartButton");
        _loadButton = _document.rootVisualElement.Q<Button>("LoadButton");
        _settingsButton = _document.rootVisualElement.Q<Button>("SettingsButton");
        _quitButton = _document.rootVisualElement.Q<Button>("QuitButton");

        // Register callbacks
        _startButton.RegisterCallback<ClickEvent>(StartGame);
        _loadButton.RegisterCallback<ClickEvent>(LoadGame);
        _settingsButton.RegisterCallback<ClickEvent>(OpenSettings);
        _quitButton.RegisterCallback<ClickEvent>(QuitGame);
        
        AudioListener.volume = PlayerPrefs.GetFloat("Volume", 1);
        
        Screen.fullScreenMode = PlayerPrefs.GetInt("Fullscreen", 1) == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }

    private void StartGame(ClickEvent ev)
    {
        GameManager.SetIsLoadedGame(false);
        SceneManager.LoadScene("PlayingScene");
    }

    private void LoadGame(ClickEvent ev)
    {
        string savePath = Application.persistentDataPath + "/save.json";

        if(!File.Exists(savePath))
        {
            _loadButton.text = "There's no saved game!";
            _loadButton.style.color = Color.white;
            _loadButton.style.backgroundColor = Color.red;

            return;
        }

        GameManager.SetIsLoadedGame(true);
        SceneManager.LoadScene("PlayingScene");
    }

    void OpenSettings(ClickEvent ev)
    {
        _settingsMenu.gameObject.SetActive(true);
    }

    private void QuitGame(ClickEvent ev)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }
}
