using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum Team { Seeker, Hider }
    public Team playerTeam;

    [Header("Game Settings")]
    public float gameDuration = 300f; // 5 minutter i sekunder
    private float timeLeft;

    [Header("UI")]
    public TMP_Text teamText;
    public TMP_Text timerText;
    public TMP_Text winText;

    [Header("Colors")]
    public Color seekerColor = Color.orange;
    public Color hiderColor = Color.cyan;

    [Header("Player Management")]
    public GameObject[] seekers;
    public GameObject[] hiders;
    private bool gameOver = false;

    void Start()
    {
        // Reset
        timeLeft = gameDuration;
        winText.gameObject.SetActive(false);

        // Random team for demonstration
        playerTeam = (Random.value > 0.5f) ? Team.Seeker : Team.Hider;

        // UI feedback
        if (teamText != null)
        {
            teamText.text = playerTeam == Team.Seeker ? "Seeker" : "Hider";
            teamText.color = playerTeam == Team.Seeker ? seekerColor : hiderColor;
        }
    }

    void Update()
    {
        if (gameOver) return;

        // Countdown timer
        timeLeft -= Time.deltaTime;
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeLeft / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        // Check win conditions
        if (timeLeft <= 0)
        {
            EndGame(Team.Hider);
        }
        else if (AllHidersCaught())
        {
            EndGame(Team.Seeker);
        }
    }

    bool AllHidersCaught()
    {
        if (hiders == null || hiders.Length == 0) return false;

        foreach (GameObject hider in hiders)
        {
            if (hider != null && hider.activeSelf) return false;
        }
        return true;
    }

    public void HiderCaught(GameObject hider)
    {
        hider.SetActive(false);
    }

    void EndGame(Team winner)
    {
        gameOver = true;
        if (winText != null)
        {
            winText.gameObject.SetActive(true);
            winText.text = $"{winner} Wins!";
            winText.color = (winner == Team.Seeker) ? seekerColor : hiderColor;
        }

        // Optional: restart after delay
        Invoke(nameof(RestartGame), 5f);
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
