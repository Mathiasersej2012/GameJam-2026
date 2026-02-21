using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TowerHealth : MonoBehaviour
{
    [Header("Tower Health")]
    [SerializeField] private int startingHealth = 10;
    [SerializeField] private string loseSceneName = "Lose";
    [SerializeField] private string winSceneName = "Win";
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Timer")]
    [SerializeField] private float survivalTimeSeconds = 120f;
    [SerializeField] private TextMeshProUGUI countdownText;

    private int currentHealth;
    private float timeRemaining;
    private bool gameEnded;

    void Start()
    {
        currentHealth = startingHealth;
        timeRemaining = survivalTimeSeconds;
        UpdateHealthText();
        UpdateCountdownText();
    }

    void Update()
    {
        if (gameEnded)
            return;

        timeRemaining -= Time.deltaTime;
        UpdateCountdownText();

        if (timeRemaining <= 0f)
        {
            gameEnded = true;
            SceneManager.LoadScene(winSceneName);
        }
    }

    public void TakeDamage(int damage)
    {
        if (gameEnded || damage <= 0)
            return;

        currentHealth -= damage;
        UpdateHealthText();

        if (currentHealth <= 0)
        {
            gameEnded = true;
            SceneManager.LoadScene(loseSceneName);
        }
    }

    private void UpdateCountdownText()
    {
        if (countdownText == null)
            return;

        float clampedTime = Mathf.Max(0f, timeRemaining);
        int minutes = Mathf.FloorToInt(clampedTime / 60f);
        int seconds = Mathf.FloorToInt(clampedTime % 60f);
        countdownText.text = $"Time Left: {minutes:00}:{seconds:00}";
    }

    private void UpdateHealthText()
    {
        if (healthText == null)
            return;

        healthText.text = $"Liv: {Mathf.Max(0, currentHealth)}";
    }
}
