using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameScene = "HomeRoom";    // must match the scene's name exactly
    public GameObject creditsPanel;          // starts hidden

    void Start()
    {
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void Play() { SceneManager.LoadScene(gameScene); }

    public void OpenCredits()  { if (creditsPanel != null) creditsPanel.SetActive(true); }
    public void CloseCredits() { if (creditsPanel != null) creditsPanel.SetActive(false); }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Update()
    {
        // Android's Back button counts as Escape: close credits first, otherwise leave the app
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (creditsPanel != null && creditsPanel.activeSelf) CloseCredits();
            else Quit();
        }
    }
}