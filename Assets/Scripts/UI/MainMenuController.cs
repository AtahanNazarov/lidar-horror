using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject optionsPanel;

    [Header("UI Elements")]
    public Slider volumeSlider; 
    public Slider sensitivitySlider; 
    public Toggle fullscreenToggle;

    private void Start()
    {
        // --- 1. SETUP VOLUME (Default 70%) ---
        float savedVol = PlayerPrefs.GetFloat("MasterVolume", 0.7f);
        AudioListener.volume = savedVol;
        
        // "SetValueWithoutNotify" moves the handle visually 
        // WITHOUT triggering the SetVolume() function below.
        if (volumeSlider != null) 
            volumeSlider.SetValueWithoutNotify(savedVol);

        // --- 2. SETUP SENSITIVITY (Default 4.0) ---
        float savedSens = PlayerPrefs.GetFloat("MouseSensitivity", 4.0f);
        if (sensitivitySlider != null)
            sensitivitySlider.SetValueWithoutNotify(savedSens);

        // --- 3. SETUP FULLSCREEN ---
        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("MainScene"); 
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenOptions()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    // --- THESE FUNCTIONS ONLY RUN WHEN *YOU* MOVE THE SLIDER ---

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}