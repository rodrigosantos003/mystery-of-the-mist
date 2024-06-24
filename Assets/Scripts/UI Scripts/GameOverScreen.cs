using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameOverScreen : MonoBehaviour
{
    private Button _quitButton;
    private Label _gameOverText;
    
    private void Awake()
    {
        _quitButton = GetComponent<UIDocument>().rootVisualElement.Q<Button>("QuitButton");
        _gameOverText = GetComponent<UIDocument>().rootVisualElement.Q<Label>("GameOverText");
        
        _quitButton.RegisterCallback<ClickEvent>(QuitGame);
    }

    public void SetLevel(int level)
    {
        if (_gameOverText == null)
            _gameOverText = GetComponent<UIDocument>().rootVisualElement.Q<Label>("GameOverText");
        _gameOverText.text = "You lost at level " + level;
    }
    
    private void QuitGame(ClickEvent ev)
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif

        Application.Quit();
    }
}
