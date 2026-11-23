using UnityEngine;

public class DebugStats : MonoBehaviour
{
    [SerializeField] string testFlowerId = "Sunflower";   // change in the Inspector to test other badges

    [ContextMenu("Add 10 coins")]  void Coins() => PlayerDataManager.Instance.AddCoins(10);
    [ContextMenu("Bloom flower")]  void Flower() => PlayerDataManager.Instance.AddFlower(testFlowerId);
    [ContextMenu("Complete goal")] void Goal()   => PlayerDataManager.Instance.CompleteGoal();

    [ContextMenu("Test: Flappy score 25")]       void TF() => PlayerDataManager.Instance.ReportFlappyScore(25);
    [ContextMenu("Test: Memory win")]            void TM() => PlayerDataManager.Instance.ReportMemoryWin(true);
    [ContextMenu("Test: Puzzle solved")]         void TP() => PlayerDataManager.Instance.ReportPuzzleSolved();
    [ContextMenu("Test: Code cracked first try")] void TC() => PlayerDataManager.Instance.ReportCodeCracked(true);

    [ContextMenu("Clear all badges and minigame stats")]
void ClearBadges()
{
    var dm = PlayerDataManager.Instance;
    var d = dm.Data;

    d.BloomedFlowers.Clear();
    d.MiniGameBadges.Clear();
    d.Flowers_Bloomed = 0;

    d.FlappyBest = 0;
    d.PuzzlesSolved = 0;
    d.CodesCracked = 0;
    d.MemoryLastDay = "";
    d.MemoryStreak = 0;
    d.ConnectLastDay = "";
    d.ConnectStreak = 0;

    dm.Save();   // also refreshes the badge slots on screen
}
}
