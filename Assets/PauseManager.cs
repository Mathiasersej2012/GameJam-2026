using StarterAssets;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI;
    public Slider sensitivitySlider;
    public Text sensitivityText;


    private bool isPaused = false;

    void Start()
    {
        // Load saved sensitivity or default to 1
        float savedSensitivity = PlayerPrefs.GetFloat("Sensitivity", 1f);
    sensitivitySlider.value = savedSensitivity;
        UpdateSensitivity(savedSensitivity);

    sensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);
    }

    void Update()
    {
        // Åbn/luk pausemenu med ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    void UpdateSensitivity(float value)
    {
        // Update text (optional)
        if (sensitivityText != null)
            sensitivityText.text = value.ToString("F2");

        // Send value to your player controller
        FirstPersonController.sensitivity = value;

        // Save it
        PlayerPrefs.SetFloat("Sensitivity", value);
    }





    public void Resume()
    {
        Debug.Log("Resume pressed");
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Time.timeScale = 1f;
        isPaused = false;
    }

    private void Pause()
    {
        Debug.Log("Game Paused");
        pauseMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void OpenOptions()
    {
        Debug.Log("Options opened");
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }

    public void CloseOptions()
    {
        Debug.Log("Options closed");
        optionsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game pressed");
        Application.Quit();
    }
}
