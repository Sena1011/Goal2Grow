using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JournalManager : MonoBehaviour
{
    [Header("Goal List")]
    public Transform goalContent;
    public GoalItemUI goalItemPrefab;

    [Header("Add Goal UI")]
    public TMP_InputField addGoalInput;
    public TMP_Dropdown addPriorityDropdown;
    public Button addButton;

    [Header("Weekly Dates")]
    public TMP_Text weekDateText;

    [Header("Weekly Progress UI")]
    public TMP_Text progressPercentText;
    public TMP_Text completedGoalsText;
    public Image progressCircle;

    [Header("Edit Goal UI")]
    public GameObject editPanel;
    public TMP_InputField editGoalInput;
    public TMP_Dropdown editPriorityDropdown;
    public Button saveEditButton;
    public Button cancelEditButton;

    [Header("Confirm Completion UI")]
    public GameObject confirmPanel;
    public TMP_Text confirmText;
    public Button confirmYesButton;
    public Button confirmNoButton;

    [Header("Growth Rules")]
    public bool priorityGivesMoreGrowth = true;   // Low 1, Medium 2, High 3
    public TMP_Text growthNoticeText;             // optional: shows "+3 growth"

    private readonly List<GoalData> goals = new List<GoalData>();
    private GoalData goalBeingEdited;
    private GoalData goalPendingComplete;
    private int completedCount;                  // goals finished this week (they leave the list)
    private string currentWeekKey;

    private void Start()
    {
        // clear the save keys left over from the removed daily goals (safe to delete these two lines later)
        PlayerPrefs.DeleteKey("JournalDailyTemplates");
        PlayerPrefs.DeleteKey("JournalDailyLastDay");

        currentWeekKey = GetWeekKey(DateTime.Today);
        UpdateWeekDate();
        LoadGoals();
        RefreshGoalList();
        UpdateProgress();

        addButton.onClick.AddListener(AddGoal);
        saveEditButton.onClick.AddListener(SaveEdit);
        cancelEditButton.onClick.AddListener(CancelEdit);

        if (confirmYesButton != null) confirmYesButton.onClick.AddListener(ConfirmComplete);
        if (confirmNoButton != null) confirmNoButton.onClick.AddListener(CancelComplete);

        if (editPanel != null) editPanel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);
        if (growthNoticeText != null) growthNoticeText.gameObject.SetActive(false);
    }

    // if the app stays open into a new week, refresh when the player comes back to it
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus || string.IsNullOrEmpty(currentWeekKey)) return;

        string key = GetWeekKey(DateTime.Today);
        if (key != currentWeekKey)
        {
            currentWeekKey = key;
            UpdateWeekDate();
            LoadGoals();
            RefreshGoalList();
            UpdateProgress();
        }
    }

    private string GetWeekKey(DateTime date)
    {
        int daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        DateTime monday = date.Date.AddDays(-daysSinceMonday);
        return monday.ToString("yyyy-MM-dd");
    }

    private void UpdateWeekDate()
    {
        int daysSinceMonday = ((int)DateTime.Today.DayOfWeek + 6) % 7;
        DateTime monday = DateTime.Today.Date.AddDays(-daysSinceMonday);
        DateTime sunday = monday.AddDays(6);

        weekDateText.text =
            monday.ToString("MMM d") + " - " +
            sunday.ToString("MMM d, yyyy");
    }

    private string SaveKey()
    {
        return "JournalGoals_" + currentWeekKey;
    }

    private string CompletedKey()
    {
        return "JournalDone_" + currentWeekKey;
    }

    private void LoadGoals()
    {
        goals.Clear();
        completedCount = PlayerPrefs.GetInt(CompletedKey(), 0);

        string json = PlayerPrefs.GetString(SaveKey(), "");
        if (string.IsNullOrEmpty(json))
            return;

        GoalListWrapper wrapper = JsonUtility.FromJson<GoalListWrapper>(json);

        if (wrapper != null && wrapper.items != null)
            goals.AddRange(wrapper.items);

        // goals ticked off under the old system count as completed and leave the list
        completedCount += goals.RemoveAll(g => g.isCompleted);
    }

    private void SaveGoals()
    {
        GoalListWrapper wrapper = new GoalListWrapper();
        wrapper.items = goals;

        PlayerPrefs.SetString(SaveKey(), JsonUtility.ToJson(wrapper));
        PlayerPrefs.SetInt(CompletedKey(), completedCount);
        PlayerPrefs.Save();
    }

    // ---------- Adding and deleting ----------

    public void AddGoal()
    {
        string text = addGoalInput.text.Trim();

        if (string.IsNullOrEmpty(text))
            return;

        int priority = addPriorityDropdown != null ? addPriorityDropdown.value : 1;
        goals.Add(new GoalData(text, priority));
        addGoalInput.text = "";

        SaveGoals();
        RefreshGoalList();
        UpdateProgress();
    }

    public void DeleteGoal(GoalData goal)
    {
        goals.Remove(goal);
        SaveGoals();
        RefreshGoalList();
        UpdateProgress();
    }

    // ---------- Completing a goal (with confirmation) ----------

    public void RequestComplete(GoalData goal)
    {
        if (confirmPanel == null)
        {
            Debug.LogError("JournalManager: Confirm Panel is not assigned.");
            return;
        }

        goalPendingComplete = goal;

        if (confirmText != null)
            confirmText.text = "Did you really finish\n\"" + goal.goalText + "\"?";

        confirmPanel.SetActive(true);
    }

    private void ConfirmComplete()
    {
        if (goalPendingComplete == null) return;

        // safety: the goal may have been deleted while the popup was open
        if (!goals.Remove(goalPendingComplete))
        {
            goalPendingComplete = null;
            if (confirmPanel != null) confirmPanel.SetActive(false);
            return;
        }

        int points = priorityGivesMoreGrowth ? goalPendingComplete.priority + 1 : 1;

        goalPendingComplete = null;
        completedCount++;

        if (Inventory.Instance != null)
        {
            Inventory.Instance.AddGrowth(points);
            ShowGrowthNotice("+" + points + " growth");
        }

        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.CompleteGoal();

        if (confirmPanel != null) confirmPanel.SetActive(false);

        RefreshGoalList();
        UpdateProgress();   // also saves
    }

    private void CancelComplete()
    {
        goalPendingComplete = null;
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    private void ShowGrowthNotice(string message)
    {
        if (growthNoticeText == null) return;
        growthNoticeText.text = message;
        growthNoticeText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideGrowthNotice));
        Invoke(nameof(HideGrowthNotice), 2f);
    }

    private void HideGrowthNotice()
    {
        if (growthNoticeText != null) growthNoticeText.gameObject.SetActive(false);
    }

    // ---------- Editing ----------

    public void OpenEditGoal(GoalData goal)
    {
        goalBeingEdited = goal;
        editGoalInput.text = goal.goalText;

        if (editPriorityDropdown != null)
            editPriorityDropdown.value = Mathf.Clamp(goal.priority, 0, 2);

        editPanel.SetActive(true);
    }

    private void SaveEdit()
    {
        if (goalBeingEdited == null)
            return;

        string text = editGoalInput.text.Trim();
        if (string.IsNullOrEmpty(text))
            return;

        goalBeingEdited.goalText = text;

        if (editPriorityDropdown != null)
            goalBeingEdited.priority = editPriorityDropdown.value;

        SaveGoals();
        RefreshGoalList();
        UpdateProgress();

        editPanel.SetActive(false);
        goalBeingEdited = null;
    }

    private void CancelEdit()
    {
        editPanel.SetActive(false);
        goalBeingEdited = null;
    }

    // ---------- Progress and list ----------

    public void UpdateProgress()
    {
        int total = goals.Count + completedCount;
        int percent = total == 0 ? 0 :
            Mathf.RoundToInt((float)completedCount / total * 100f);

        progressPercentText.text = percent + "%";
        completedGoalsText.text =
            completedCount + "/" + total + " Goals Completed";

        if (progressCircle != null)
            progressCircle.fillAmount = percent / 100f;

        SaveGoals();
    }

    private void RefreshGoalList()
    {
        foreach (Transform child in goalContent)
            Destroy(child.gameObject);

        // High priority first
        foreach (GoalData goal in goals.OrderByDescending(g => g.priority))
        {
            GoalItemUI item = Instantiate(goalItemPrefab, goalContent);
            item.transform.localScale = Vector3.one;
            item.Setup(goal, this);
        }
    }

    [Serializable]
    private class GoalListWrapper
    {
        public List<GoalData> items = new List<GoalData>();
    }
}