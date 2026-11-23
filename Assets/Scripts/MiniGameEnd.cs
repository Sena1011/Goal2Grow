using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameEnd : MonoBehaviour
{
    public int rewardCoins = 50;
    public GameObject winPanel;
    public TMP_Text winText;
    public string MinigameMenuName = "MiniGame Menu";   // must match scene name exactly

    bool rewarded;

    void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    public enum GameKind { None, Memory, Connect, CodeHunt }
    public GameKind game;   // pick this in the Inspector for each scene

    public void Win() { Win(false, false); }   // keeps existing buttons and scripts working

    public void Win(bool perfectRound, bool firstTry)
    {
        if (rewarded) return;   // only pays once per round
        rewarded = true;

        if (Inventory.Instance != null)
            Inventory.Instance.AddMoney(rewardCoins);

        var dm = PlayerDataManager.Instance;
        if (dm != null)
        {
            switch (game)
            {
                case GameKind.Memory:   dm.ReportMemoryWin(perfectRound); break;
                case GameKind.Connect:  dm.ReportPuzzleSolved(); break;
                case GameKind.CodeHunt: dm.ReportCodeCracked(firstTry); break;
            }
        }

        if (winText != null)
            winText.text = "You won!\n+" + rewardCoins + " coins";

        if (winPanel != null)
            winPanel.SetActive(true);
    }

    public void ResetForNewRound()
    {
        rewarded = false;
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoHome()
    {
        SceneManager.LoadScene(MinigameMenuName);
    }
}