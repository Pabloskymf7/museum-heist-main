using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        if (playButton == null)
            playButton = GameObject.Find("PlayButton")?.GetComponent<Button>();
        if (quitButton == null)
            quitButton = GameObject.Find("QuitButton")?.GetComponent<Button>();

        playButton?.onClick.AddListener(OnPlayButton);
        quitButton?.onClick.AddListener(OnQuitButton);
    }

    public void OnPlayButton()
    {
        SceneManager.LoadScene(1); // Level1 es Build Index 1
    }

    public void OnQuitButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}