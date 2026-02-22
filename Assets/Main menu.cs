using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Canvas references")]
    [Tooltip("Assign the Main Menu canvas GameObject here.")]
    public GameObject mainMenuCanvas;
    [Tooltip("Assign the other canvas (e.g. Options) to show when main menu is hidden.")]
    public GameObject otherCanvas;

    // Called when pressing "Start"
    public void StartGame()
    {
        SceneManager.LoadScene("Sample  Scene");
        // Husk at tilføje scenen i Build Settings
    }

    // Called when pressing "Options" from UI - hides main menu and shows the assigned otherCanvas
    public void OpenOptions()
    {
        SwitchToCanvas(otherCanvas);
    }

    // Called when pressing "Quit"
    public void QuitGame()
    {
        Debug.Log("Spillet lukkes...");
        Application.Quit();
    }

    // Hides the main menu canvas and shows the provided canvas.
    // If provided canvas is null it will only hide the main menu.
    public void SwitchToCanvas(GameObject canvasToShow)
    {
        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(false);
        else
            Debug.LogWarning("MainMenu.SwitchToCanvas: mainMenuCanvas is not assigned.");

        if (canvasToShow != null)
            canvasToShow.SetActive(true);
    }

    // Optional helper to go back to main menu (hides other and shows main)
    public void ShowMainMenu()
    {
        if (otherCanvas != null)
            otherCanvas.SetActive(false);

        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(true);
    }
}
