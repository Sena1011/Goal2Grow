using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public string Player_Name = "Jana";
    public string Bio = "Small steps, big bloom!";
    public int Coins = 00;
    public int Flowers_Bloomed = 00;
    public int Goals_Completed = 00;

    public List<string> BloomedFlowers = new List<string>();
    public List<string> MiniGameBadges = new List<string>();

    public int FlappyBest;
    public int PuzzlesSolved;
    public int CodesCracked;
    public string MemoryLastDay = "";
    public int MemoryStreak;
    public string ConnectLastDay = "";
    public int ConnectStreak;
}