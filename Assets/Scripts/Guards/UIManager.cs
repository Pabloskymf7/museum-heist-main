using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private GameObject gameOverPanel;
    private GameObject victoryPanel;
    private Button retryButton;
    private Button nextLevelButton;
    private Button mainMenuButton;

    private void Awake()
    {
        gameOverPanel   = FindByName("GameOverPanel");
        victoryPanel    = FindByName("VictoryPanel");
        retryButton     = FindByName("RetryButton")?.GetComponent<Button>();
        nextLevelButton = FindByName("NextLevelButton")?.GetComponent<Button>();
        mainMenuButton  = FindByName("MainMenuButton")?.GetComponent<Button>();

        gameOverPanel?.SetActive(false);
        victoryPanel?.SetActive(false);

        retryButton?.onClick.AddListener(OnRetry);
        nextLevelButton?.onClick.AddListener(OnNextLevel);
        mainMenuButton?.onClick.AddListener(OnMainMenu);
    }

    private GameObject FindByName(string name)
    {
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name == name) return t.gameObject;
        return null;
    }

    public void ShowGameOver() => gameOverPanel?.SetActive(true);
    public void ShowVictory()  => victoryPanel?.SetActive(true);

    private void OnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnNextLevel()
    {
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(next < SceneManager.sceneCountInBuildSettings ? next : 0);
    }

    private void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}