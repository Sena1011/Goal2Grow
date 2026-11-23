using System;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerDataManager : MonoBehaviour
{

    public static PlayerDataManager Instance {get; private set; }
    public PlayerData Data {get; private set; }
    public event Action OnChanged;

    string SavePath => Path.Combine(Application.persistentDataPath, "player.json");
    string AvatarPath => Path.Combine(Application.persistentDataPath, "avatar.jpg");

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    void Load()
    {
        Data = File.Exists(SavePath)
            ? JsonUtility.FromJson<PlayerData>(File.ReadAllText(SavePath))
            : new PlayerData();
    }

    public void Save()
    {
        File.WriteAllText(SavePath, JsonUtility.ToJson(Data));
        OnChanged?.Invoke();
    }

    public void SetName(string n) {Data.Player_Name = n; Save(); }
    public void SetBio(string b) {Data.Bio = b; Save(); }
    public void AddCoins(int n) {Data.Coins += n; Save(); }
    public event Action<string> OnFlowerBadgeUnlocked;

    public void AddFlower(string flowerId)
    {
        Data.Flowers_Bloomed++;
        bool isNew = !Data.BloomedFlowers.Contains(flowerId);
        if (isNew) Data.BloomedFlowers.Add(flowerId);
        Save();
        if (isNew) OnFlowerBadgeUnlocked?.Invoke(flowerId);   // only the first time that flower blooms
    }  

    public bool HasBloomed(string flowerId)
    {
        return Data.BloomedFlowers.Contains(flowerId);
    }

    public event Action<string> OnMiniGameBadgeUnlocked;

    public bool HasMiniGameBadge(string id)
    {
        return Data.MiniGameBadges.Contains(id);
    }

    public void UnlockMiniGameBadge(string id)
    {
        if (Data.MiniGameBadges.Contains(id)) return;   // only the first time
        Data.MiniGameBadges.Add(id);
        Save();
        OnMiniGameBadgeUnlocked?.Invoke(id);
    }

    public void CompleteGoal() {Data.Goals_Completed++; Save(); }

    public void SaveAvatar(Texture2D tex)
    {
        File.WriteAllBytes(AvatarPath, tex.EncodeToPNG());
        OnChanged?. Invoke();
    }

    public Texture2D LoadAvatar()
    {
        if (!File.Exists(AvatarPath)) return null;
        var tex = new Texture2D(2, 2);
        tex.LoadImage(File.ReadAllBytes(AvatarPath));
        return tex;
    }

    public void ResetProgress()
    {
        Data = new PlayerData();
        if (File.Exists(AvatarPath)) File.Delete(AvatarPath);
        Save();
    }

    // ---------- Minigame badge tracking ----------
    // you will slowly see my decay into madness

    static readonly int[] FlappyScores = { 5, 10, 25, 50, 100 };
    static readonly string[] FlappyIds = { "FlappyLittleFlyer", "FlappySkyAdventurer", "FlappyFeatheredChampion", "FlappySkyMaster", "FlappyBirdLegend" };

    static readonly int[] PuzzleCounts = { 1, 10, 25, 50 };
    static readonly string[] PuzzleIds = { "ConnectFirstConnection", "ConnectDotExplorer", "ConnectPatternFinder", "ConnectPuzzlePro" };

    static readonly int[] CodeCounts = { 25, 50, 100, 200 };
    static readonly string[] CodeIds = { "CodeCracker", "CodeMasterDecoder", "CodeSpecialist", "CodeUltimateHacker" };

    // counts days in a row; skipping a day starts the streak again at 1
    int UpdateStreak(ref string lastDay, ref int streak)
    {
        //WHY THE FUCK DO YOU WORK NOW???
        //YOU DIDNT WORK A MINUTE AGO
        string today = DateTime.Today.ToString("yyyy-MM-dd");
        //never touching this shit again.
        if (lastDay == today) return streak;                      // already counted today
        string yesterday = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
        streak = (lastDay == yesterday) ? streak + 1 : 1;
        lastDay = today;
        return streak;
    }

    public void ReportFlappyScore(int score)
    {
        if (score > Data.FlappyBest) { Data.FlappyBest = score; Save(); }
        for (int i = 0; i < FlappyScores.Length; i++)
        if (score >= FlappyScores[i]) UnlockMiniGameBadge(FlappyIds[i]);
    }

    public void ReportMemoryWin(bool perfectRound)
    {
        UnlockMiniGameBadge("MemoryFirstMatch");
        if (perfectRound) UnlockMiniGameBadge("MemoryPerfect");

        //so many resets just to see if these work.
        int streak = UpdateStreak(ref Data.MemoryLastDay, ref Data.MemoryStreak);
        Save();
        if (streak >= 3) UnlockMiniGameBadge("MemoryDailyThinker");
        if (streak >= 7) UnlockMiniGameBadge("MemoryMindChampion");
    }

    public void ReportPuzzleSolved()
    {
        Data.PuzzlesSolved++;
        int streak = UpdateStreak(ref Data.ConnectLastDay, ref Data.ConnectStreak);
        Save();

        for (int i = 0; i < PuzzleCounts.Length; i++)
        if (Data.PuzzlesSolved >= PuzzleCounts[i]) UnlockMiniGameBadge(PuzzleIds[i]);
        if (streak >= 3) UnlockMiniGameBadge("ConnectDailyConnect");
        if (streak >= 7) UnlockMiniGameBadge("ConnectConnectingFlower");
    }

    public void ReportCodeCracked(bool firstTry)
    {
        Data.CodesCracked++;
        Save();

        if (firstTry) UnlockMiniGameBadge("CodeOneShotWonder");
        for (int i = 0; i < CodeCounts.Length; i++)
        if (Data.CodesCracked >= CodeCounts[i]) UnlockMiniGameBadge(CodeIds[i]);
    }
}