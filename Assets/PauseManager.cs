using StarterAssets;
using UnityEngine;
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

        if (sensitivitySlider != null)
            sensitivitySlider.value = savedSensitivity;

        UpdateSensitivity(savedSensitivity);

        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);
        else
            Debug.LogWarning("PauseMenu: sensitivitySlider is not assigned.");
    }

    void OnDestroy()
    {
        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.RemoveListener(UpdateSensitivity);
    }

    void Update()
    {
        // Open/close pause menu with ESC
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
        if (sensitivityText != null)
            sensitivityText.text = value.ToString("F2");

        FirstPersonController.sensitivity = value;
        PlayerPrefs.SetFloat("Sensitivity", value);
    }

    public void Resume()
    {
        Debug.Log("Resume pressed");

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
        if (optionsMenuUI != null)
            optionsMenuUI.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Time.timeScale = 1f;
        isPaused = false;
    }

    private void Pause()
    {
        Debug.Log("Game Paused");

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
        if (optionsMenuUI != null)
            optionsMenuUI.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void OpenOptions()
    {
        Debug.Log("Options opened");

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
        if (optionsMenuUI != null)
            optionsMenuUI.SetActive(true);
    }

    public void CloseOptions()
    {
        Debug.Log("Options closed");

        if (optionsMenuUI != null)
            optionsMenuUI.SetActive(false);
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game pressed");
        Application.Quit();
    }
}
