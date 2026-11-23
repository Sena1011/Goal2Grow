using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalItemUI : MonoBehaviour
{
    [Header("Goal Display")]
    public TMP_Text goalText;
    public TMP_Text statusText;      // now shows the priority

    [Header("Priority Appearance")]
    public Image statusBox;
    public Color lowColor = new Color32(217, 234, 215, 255);
    public Color mediumColor = new Color32(255, 236, 179, 255);
    public Color highColor = new Color32(248, 187, 182, 255);

    [Header("Buttons")]
    public Button completeButton;
    public Button editButton;
    public Button deleteButton;

    [Header("Checkmark Image")]
    public GameObject checkmark;     // not used anymore, can stay empty

    private GoalData goalData;
    public GameObject dailyBadge;   // small "Daily" label on the row
    private JournalManager journalManager;

    public void Setup(GoalData data, JournalManager manager)
    {
        goalData = data;
        journalManager = manager;

        completeButton.onClick.RemoveListener(CompleteGoal);
        editButton.onClick.RemoveListener(EditGoal);
        deleteButton.onClick.RemoveListener(DeleteGoal);

        completeButton.onClick.AddListener(CompleteGoal);
        editButton.onClick.AddListener(EditGoal);
        deleteButton.onClick.AddListener(DeleteGoal);

        Refresh();
    }

    public void Refresh()
    {
        if (goalData == null) return;

        string[] names = { "Low", "Medium", "High" };
        Color[] colors = { lowColor, mediumColor, highColor };
        int p = Mathf.Clamp(goalData.priority, 0, 2);

        goalText.text = goalData.goalText;
        statusText.text = names[p];
        statusBox.color = colors[p];

        if (checkmark != null)
            checkmark.SetActive(false);

        if (dailyBadge != null) dailyBadge.SetActive(goalData.IsDaily);
    }

    private void CompleteGoal()
    {
        journalManager.RequestComplete(goalData);   // opens the "did you finish?" popup
    }

    private void EditGoal()
    {
        journalManager.OpenEditGoal(goalData);
    }

    private void DeleteGoal()
    {
        journalManager.DeleteGoal(goalData);
    }
}