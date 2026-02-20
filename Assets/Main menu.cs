using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Kaldet når man trykker på "Start"
    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
        // Husk at tilføje scenen i Build Settings
    }

    // Kaldet når man trykker på "Options"
    public void OpenOptions()
    {
        // Her kan du åbne en options-menu, aktivere et panel osv.
        Debug.Log("Options menu åbnes...");
    }

    // Kaldet når man trykker på "Quit"
    public void QuitGame()
    {
        Debug.Log("Spillet lukkes...");
        Application.Quit();
    }
}
