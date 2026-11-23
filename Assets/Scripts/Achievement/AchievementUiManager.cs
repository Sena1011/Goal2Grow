using UnityEngine;

public class AchievementUIManager : MonoBehaviour
{
    [Header("Achievement UI")]
    public GameObject achievementPanel;

    [Header("Badge Pages")]
    public GameObject flowerBadgePage;
    public GameObject miniGameBadgePage;

    public void OpenAchievements()
    {
        achievementPanel.SetActive(true);
        ShowFlowerBadges();
    }

    public void ShowFlowerBadges()
    {
        flowerBadgePage.SetActive(true);
        miniGameBadgePage.SetActive(false);
    }

    
    public void ShowMiniGameBadges()
    {
        flowerBadgePage.SetActive(false);
        miniGameBadgePage.SetActive(true);
    }

    public void CloseAchievements()
    {
        achievementPanel.SetActive(false);
    }
}