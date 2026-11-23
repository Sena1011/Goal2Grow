using System;

[Serializable]
public class GoalData
{
    public string goalText;
    public int priority = 1;      // 0 = Low, 1 = Medium, 2 = High
    public bool isCompleted;      // kept so goals saved before this update still load
    public string dailyId = "";                         // empty = normal goal
    public bool IsDaily => !string.IsNullOrEmpty(dailyId);

    public GoalData(string text, int priority = 1)
    {
        goalText = text;
        this.priority = priority;
    }
}