using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
{
    // Fallback por si las referencias se pierden
    if (mainMenuButton == null)
        mainMenuButton = GameObject.Find("MainMenuButton")?.GetComponent<Button>();
    if (retryButton == null)
        retryButton = GameObject.Find("RetryButton")?.GetComponent<Button>();
    if (nextLevelButton == null)
        nextLevelButton = GameObject.Find("NextLevelButton")?.GetComponent<Button>();

    gameOverPanel?.SetActive(false);
    victoryPanel?.SetActive(false);

    retryButton?.onClick.AddListener(OnRetry);
    nextLevelButton?.onClick.AddListener(OnNextLevel);
    mainMenuButton?.onClick.AddListener(OnMainMenu);
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