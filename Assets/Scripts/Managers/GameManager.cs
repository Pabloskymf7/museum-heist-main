using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public enum GameState { Playing, Won, Lost }
    public GameState State { get; private set; } = GameState.Playing;
    public bool hasJewel = false;

    [SerializeField] private UIManager uiManager;

    private void Awake()
    {
        Instance = this;
        hasJewel = false;
        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();
    }

    public void PlayerDetected()
    {
        if (State != GameState.Playing) return;
        State = GameState.Lost;
        Debug.Log("GameManager: Game Over.");
        uiManager?.ShowGameOver();
        StartCoroutine(ReloadAfterDelay(3f));
    }

    public void PlayerWon()
    {
        if (State != GameState.Playing) return;
        State = GameState.Won;
        Debug.Log("GameManager: Player won!");
        AudioManager.Instance?.PlayVictory();
        uiManager?.ShowVictory();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
{
    Time.timeScale = 1f;
    SceneManager.LoadScene("MainMenu");
}

    private IEnumerator ReloadAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RestartLevel();
    }
}