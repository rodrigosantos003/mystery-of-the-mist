using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class WinScreen : MonoBehaviour
{
    private Button _quitButton;
    
    private void Awake()
    {
        _quitButton = GetComponent<UIDocument>().rootVisualElement.Q<Button>("QuitButton");
        
        _quitButton.RegisterCallback<ClickEvent>(QuitGame);
    }
    
    private void QuitGame(ClickEvent ev)
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif

        Application.Quit();
    }
}
