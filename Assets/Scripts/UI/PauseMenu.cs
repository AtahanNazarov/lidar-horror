using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject optionsPanel;

    [Header("Settings Controls")]
    public Slider volumeSlider;       
    public Slider sensitivitySlider;  
    public MouseLook playerMouseLook; 

    public static bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);
        // Important: Force options active briefly so sliders render correctly
        optionsPanel.SetActive(true); 

        // --- 1. SYNC VOLUME (Default 70%) ---
        float savedVol = PlayerPrefs.GetFloat("MasterVolume", 0.7f);
        AudioListener.volume = savedVol; 
        
        // Update visual handle silently
        if (volumeSlider != null) 
            volumeSlider.SetValueWithoutNotify(savedVol);

        // --- 2. SYNC SENSITIVITY (Default 4.0) ---
        float savedSens = PlayerPrefs.GetFloat("MouseSensitivity", 4.0f);
        
        // Update the actual camera speed
        if (playerMouseLook != null) 
            playerMouseLook.mouseSensitivity = savedSens;
            
        // Update visual handle silently
        if (sensitivitySlider != null) 
            sensitivitySlider.SetValueWithoutNotify(savedSens);

        // Hide panels again
        optionsPanel.SetActive(false); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f; 
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        Time.timeScale = 1f; 
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    // --- SETTINGS (Only runs when YOU drag) ---

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSensitivity(float sensitivity)
    {
        if (playerMouseLook != null)
            playerMouseLook.mouseSensitivity = sensitivity;
            
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}