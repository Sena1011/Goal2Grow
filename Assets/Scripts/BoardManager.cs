using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [System.Serializable]
    public class DotPair
    {
        public Color color = Color.white;
        public Vector2Int start;
        public Vector2Int end;
    }

    [Header("Grid Settings")]
    public int columns = 5;
    public int rows = 5;
    public float cellSize = 1f; // should match your Pixels Per Unit setup (1 tile = 1 unit)

    [Header("Prefabs (assign in Inspector)")]
    public GameObject dotPrefab; // a prefab with a SpriteRenderer using your grayscale/white flower sprite

    [Header("Puzzle Data")]
    public List<DotPair> dotPairs = new List<DotPair>();

    [Header("Random Puzzle Generation (optional)")]
    [Tooltip("If true, ignores the manually-set Dot Pairs above and generates a random solvable layout instead")]
    public bool randomizePuzzle = false;

    public enum Difficulty { Easy, Normal, Hard }
    [Tooltip("Applies preset grid size, pair count, and path length values when Randomize Puzzle is checked")]
    public Difficulty difficulty = Difficulty.Normal;

    public int randomPairCount = 5;
    [Tooltip("Minimum/maximum number of cells each generated path occupies (including both dot endpoints)")]
    public int minPathLength = 2;
    public int maxPathLength = 6;
    [Tooltip("Colors to assign to generated pairs, in order. Add more if randomPairCount is high.")]
    public Color[] palette = new Color[] { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan };

    // Tracks what's in each cell so path logic can check it later
    public enum CellContent { Empty, Dot, Path }
    private Dictionary<Vector2Int, CellContent> cellStates = new Dictionary<Vector2Int, CellContent>();
    private Dictionary<Vector2Int, Color> dotColors = new Dictionary<Vector2Int, Color>();
    private Dictionary<Vector2Int, int> dotPairIndex = new Dictionary<Vector2Int, int>();

    void Start()
    {
        if (randomizePuzzle)
        {
            ApplyDifficultyPreset(difficulty);
        }
        SetupGrid();
        if (randomizePuzzle)
        {
            dotPairs = GenerateRandomDotPairs(randomPairCount);
        }
        SpawnDots();
    }

    // Overwrites columns/rows/randomPairCount/path length with preset values per difficulty.
    // Feel free to tweak these numbers to taste - they're just reasonable starting points.
    void ApplyDifficultyPreset(Difficulty level)
    {
        switch (level)
        {
            case Difficulty.Easy:
                columns = 5;
                rows = 5;
                randomPairCount = 4;
                minPathLength = 2;
                maxPathLength = 4;
                break;

            case Difficulty.Normal:
                columns = 6;
                rows = 6;
                randomPairCount = 6;
                minPathLength = 3;
                maxPathLength = 6;
                break;

            case Difficulty.Hard:
                columns = 8;
                rows = 8;
                randomPairCount = 9;
                minPathLength = 4;
                maxPathLength = 9;
                break;
        }
    }

    void SetupGrid()
    {
        cellStates.Clear();
        dotColors.Clear();
        dotPairIndex.Clear();
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                cellStates[new Vector2Int(x, y)] = CellContent.Empty;
            }
        }
    }

    void SpawnDots()
    {
        for (int i = 0; i < dotPairs.Count; i++)
        {
            DotPair pair = dotPairs[i];
            SpawnSingleDot(pair.start, pair.color, i);
            SpawnSingleDot(pair.end, pair.color, i);
        }
    }

    void SpawnSingleDot(Vector2Int gridPos, Color color, int pairIndex)
    {
        if (dotPrefab == null)
        {
            Debug.LogWarning("BoardManager: dotPrefab is not assigned in the Inspector.");
            return;
        }

        Vector3 worldPos = GridToWorld(gridPos);
        GameObject dotObj = Instantiate(dotPrefab, worldPos, Quaternion.identity, transform);

        SpriteRenderer sr = dotObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = color; // this only tints correctly if the sprite itself is grayscale/white
        }

        cellStates[gridPos] = CellContent.Dot;
        dotColors[gridPos] = color;
        dotPairIndex[gridPos] = pairIndex;
    }

    public Color GetDotColorAt(Vector2Int gridPos)
    {
        if (dotColors.TryGetValue(gridPos, out Color c))
            return c;
        return Color.white;
    }

    public int GetDotPairIndexAt(Vector2Int gridPos)
    {
        if (dotPairIndex.TryGetValue(gridPos, out int idx))
            return idx;
        return -1;
    }

    // Converts a grid coordinate (e.g. 2,3) into a real Unity world position
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        // Centers the grid around the origin - adjust if you want it positioned elsewhere
        float offsetX = (columns - 1) * cellSize * 0.5f;
        float offsetY = (rows - 1) * cellSize * 0.5f;
        return new Vector3(gridPos.x * cellSize - offsetX, gridPos.y * cellSize - offsetY, 0f);
    }

    // Call this from a UI button (e.g. difficulty select buttons) to rebuild the board at runtime
    public void RegenerateBoard(Difficulty newDifficulty)
    {
        // Remove any previously spawned dots before rebuilding
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        difficulty = newDifficulty;
        randomizePuzzle = true;
        ApplyDifficultyPreset(difficulty);
        SetupGrid();
        dotPairs = GenerateRandomDotPairs(randomPairCount);
        SpawnDots();
    }

    public CellContent GetCellContent(Vector2Int gridPos)
    {
        if (cellStates.TryGetValue(gridPos, out CellContent content))
            return content;
        return CellContent.Empty;
    }

    // Builds a random, guaranteed-solvable set of dot pairs by drawing an actual
    // random path for each pair first, then placing the dots at its two ends.
    // Because each walk only steps onto cells nothing else has claimed yet,
    // the puzzle is always solvable - the walk itself is a valid solution.
    List<DotPair> GenerateRandomDotPairs(int count)
    {
        List<DotPair> result = new List<DotPair>();
        HashSet<Vector2Int> claimed = new HashSet<Vector2Int>();

        List<Vector2Int> allCells = new List<Vector2Int>();
        for (int x = 0; x < columns; x++)
            for (int y = 0; y < rows; y++)
                allCells.Add(new Vector2Int(x, y));

        int colorIndex = 0;
        int totalAttempts = 0;
        int maxTotalAttempts = count * 50; // generous retry budget in case a walk dead-ends early

        while (result.Count < count && totalAttempts < maxTotalAttempts)
        {
            totalAttempts++;

            // Find an unclaimed cell to start this pair's path from
            List<Vector2Int> available = new List<Vector2Int>();
            foreach (var c in allCells)
                if (!claimed.Contains(c)) available.Add(c);

            if (available.Count == 0) break; // board is full, can't fit any more pairs

            Vector2Int start = available[Random.Range(0, available.Count)];
            int targetLength = Random.Range(minPathLength, maxPathLength + 1);
            List<Vector2Int> path = RandomWalk(start, targetLength, claimed);

            if (path.Count >= minPathLength)
            {
                foreach (var c in path) claimed.Add(c);

                Color color = palette.Length > 0 ? palette[colorIndex % palette.Length] : Random.ColorHSV();
                colorIndex++;

                result.Add(new DotPair
                {
                    color = color,
                    start = path[0],
                    end = path[path.Count - 1]
                });
            }
            // if the walk was too short, we just don't claim anything and try again elsewhere
        }

        if (result.Count < count)
        {
            Debug.LogWarning($"BoardManager: only generated {result.Count}/{count} pairs - grid may be too small or maxPathLength too high for this many pairs.");
        }

        return result;
    }

    List<Vector2Int> RandomWalk(Vector2Int start, int targetLength, HashSet<Vector2Int> claimed)
    {
        List<Vector2Int> path = new List<Vector2Int> { start };
        HashSet<Vector2Int> visited = new HashSet<Vector2Int> { start };

        while (path.Count < targetLength)
        {
            Vector2Int current = path[path.Count - 1];
            List<Vector2Int> neighbors = GetShuffledNeighborDirections();

            Vector2Int? next = null;
            foreach (var dir in neighbors)
            {
                Vector2Int candidate = current + dir;
                if (candidate.x < 0 || candidate.x >= columns || candidate.y < 0 || candidate.y >= rows) continue;
                if (claimed.Contains(candidate) || visited.Contains(candidate)) continue;

                // Reject this candidate if it would run right alongside an earlier part of this
                // same path (touching it on a side other than where we just came from) - this is
                // what caused the "ladder" look, where the path folds back parallel to itself.
                if (IsAdjacentToPathExceptCurrent(candidate, current, visited)) continue;

                next = candidate;
                break;
            }

            if (next == null) break; // dead end - stop here, path may end up shorter than targetLength
            path.Add(next.Value);
            visited.Add(next.Value);
        }

        return path;
    }

    // Checks whether 'candidate' touches any cell already in the path, other than 'current'
    // (which is expected and fine, since that's the step we're moving from).
    bool IsAdjacentToPathExceptCurrent(Vector2Int candidate, Vector2Int current, HashSet<Vector2Int> visited)
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var dir in dirs)
        {
            Vector2Int neighbor = candidate + dir;
            if (neighbor == current) continue; // the step we're coming from is always allowed
            if (visited.Contains(neighbor)) return true;
        }
        return false;
    }

    List<Vector2Int> GetShuffledNeighborDirections()
    {
        List<Vector2Int> dirs = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        for (int i = dirs.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (dirs[i], dirs[j]) = (dirs[j], dirs[i]);
        }
        return dirs;
    }
}