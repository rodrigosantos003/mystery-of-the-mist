using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SettingsMenu : MonoBehaviour
{
    private UIDocument _document;

    private Slider _volumeSlider;
    private DropdownField _resolutionField;
    private Toggle _fullscreenToggle;
    private Button _backButton;

    private Resolution[] resolutions;

    // Start is called before the first frame update
    void OnEnable()
    {
        _document = GetComponent<UIDocument>();
        _volumeSlider = _document.rootVisualElement.Q<Slider>("Volume");
        _resolutionField = _document.rootVisualElement.Q<DropdownField>("Resolution");
        _fullscreenToggle = _document.rootVisualElement.Q<Toggle>("Fullscreen");
        _backButton = _document.rootVisualElement.Q<Button>("BackButton");

        resolutions = Screen.resolutions;

        _resolutionField.choices.Clear();
        foreach (Resolution resolution in resolutions)
        {
            _resolutionField.choices.Add(resolution.ToString());
        }

        _resolutionField.value = resolutions[PlayerPrefs.GetInt("Resolution", resolutions.Length - 1)].ToString();
        _resolutionField.RegisterValueChangedCallback(ev => ChangeResolution(ev.newValue));
        
        _volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1);
        _volumeSlider.RegisterValueChangedCallback(ev => SetVolume(ev.newValue));
        
        _fullscreenToggle.value = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        _fullscreenToggle.RegisterValueChangedCallback(ev => SetFullscreen(ev.newValue));
        
        _backButton.RegisterCallback<ClickEvent>(GoBack);
        
        LoadSettings();
    }

    void ChangeResolution(string value)
    {
        int index = _resolutionField.choices.IndexOf(value);

        if (index >= 0 && index < resolutions.Length)
        {
            PlayerPrefs.SetInt("Resolution", index);
            Screen.SetResolution(resolutions[index].width, resolutions[index].height, PlayerPrefs.GetInt("Fullscreen", 1) == 1);
        }
        else
        {
            Debug.LogWarning("Invalid resolution index.");
        }   
    }

    void SetVolume(float volume)
    {
        PlayerPrefs.SetFloat("Volume", volume);
        AudioListener.volume = volume;
    }

    void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    void GoBack(ClickEvent ev)
    {
        gameObject.SetActive(false);
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey("Fullscreen"))
        {
            Screen.fullScreenMode = PlayerPrefs.GetInt("Fullscreen", 1) == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        }

        if (PlayerPrefs.HasKey("Volume"))
        {
            AudioListener.volume = PlayerPrefs.GetFloat("Volume", 1);
        }

        if (PlayerPrefs.HasKey("Resolution"))
        {
            int resolutionIndex = PlayerPrefs.GetInt("Resolution");
            if (resolutionIndex < Screen.resolutions.Length)
            {
                Screen.SetResolution(Screen.resolutions[resolutionIndex].width, Screen.resolutions[resolutionIndex].height, PlayerPrefs.GetInt("Fullscreen", 1) == 1);
            }
        }
    }
}
