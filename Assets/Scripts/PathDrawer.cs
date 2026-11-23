using System.Collections.Generic;
using UnityEngine;

// Handles the player dragging from one dot to any matching-colored dot,
// spawning and rotating straight/corner stem pieces along the way.
//
// Rules enforced (kept simple on purpose):
// - You can't move onto a cell already used by your own path, or by another
//   color's path, or onto a dot of a different color. If the cell you're
//   trying to enter is blocked, the drag simply stays put - no move happens.
// - Dragging stops automatically once it reaches a connecting dot.
// - Any dot of the same color completes the path, regardless of which
//   "pair" it was originally generated as.
public class PathDrawer : MonoBehaviour
{
    [Header("References")]
    public BoardManager board;

    [Header("Win")]
    public MinigameEnd minigameEnd;
    private bool won = false;

    [Header("Stem Prefabs (assign in Inspector)")]
    public GameObject straightPrefab;
    public GameObject cornerPrefab;

    [Header("Drag Indicator (optional)")]
    [Tooltip("A small sprite that follows the current path-head cell while dragging - assign a grayscale/white sprite so it can be tinted per color")]
    public GameObject dragIndicatorPrefab;

    private GameObject activeDragIndicator;

    [Header("Rotation Tuning")]
    [Tooltip("Add 90/180/270 here if straight pieces look rotated wrong")]
    public float straightRotationOffset = 0f;
    [Tooltip("Add 90/180/270 here if corner pieces look rotated wrong")]
    public float cornerRotationOffset = 0f;
    [Tooltip("Adjust if your drag indicator sprite doesn't point right (0 degrees) by default")]
    public float dragIndicatorRotationOffset = 0f;

    

    private class ActivePath
    {
        public int pairIndex;
        public Color32 colorKey;
        public Color color;
        public List<Vector2Int> cells = new List<Vector2Int>();
        public List<GameObject> segmentObjects = new List<GameObject>();
    }

    private class CommittedPath
    {
        public Color32 colorKey;
        public List<Vector2Int> cells;
        public List<GameObject> visuals;
        public bool connected;
    }

    private ActivePath currentPath;
    private bool isDragging = false;

    // Keyed by pair index (not color) so two different pairs sharing a color don't
    // erase each other when redrawing - but each entry still records its color,
    // since that's what the connection/blocking checks actually care about.
    private Dictionary<int, CommittedPath> committedPaths = new Dictionary<int, CommittedPath>();

    void Update()
    {
        if (won) return; 

        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            ContinueDrag();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag();
        }
    }

    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float offsetX = (board.columns - 1) * board.cellSize * 0.5f;
        float offsetY = (board.rows - 1) * board.cellSize * 0.5f;
        int gx = Mathf.RoundToInt((worldPos.x + offsetX) / board.cellSize);
        int gy = Mathf.RoundToInt((worldPos.y + offsetY) / board.cellSize);
        return new Vector2Int(gx, gy);
    }

    void TryStartDrag()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2Int gridPos = WorldToGrid(mouseWorld);

        if (board.GetCellContent(gridPos) != BoardManager.CellContent.Dot) return;

        Color dotColor = board.GetDotColorAt(gridPos);
        int pairIndex = board.GetDotPairIndexAt(gridPos);

        // If this specific pair already has a path drawn, clear it out so the player can redraw -
        // note this only clears THIS pair's own previous path, not other pairs sharing the same color.
        if (committedPaths.TryGetValue(pairIndex, out CommittedPath old))
        {
            foreach (var obj in old.visuals) Destroy(obj);
            committedPaths.Remove(pairIndex);
        }

        currentPath = new ActivePath { color = dotColor, colorKey = dotColor, pairIndex = pairIndex };
        currentPath.cells.Add(gridPos);
        isDragging = true;
        Debug.Log($"Started drag at {gridPos} with color {dotColor} (pair {pairIndex})");

        if (dragIndicatorPrefab != null)
        {
            Vector3 spawnPos = board.GridToWorld(gridPos);
            activeDragIndicator = Instantiate(dragIndicatorPrefab, spawnPos, Quaternion.identity, transform);
            SpriteRenderer indicatorSr = activeDragIndicator.GetComponent<SpriteRenderer>();
            if (indicatorSr != null) indicatorSr.color = dotColor;
        }
    }

    void ContinueDrag()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2Int gridPos = WorldToGrid(mouseWorld);

        Vector2Int lastCell = currentPath.cells[currentPath.cells.Count - 1];
        if (gridPos == lastCell) return; // hasn't moved to a new cell yet

        // Only allow moving to an orthogonally adjacent cell (no diagonals)
        int dx = Mathf.Abs(gridPos.x - lastCell.x);
        int dy = Mathf.Abs(gridPos.y - lastCell.y);
        if (dx + dy != 1) return;

        if (gridPos.x < 0 || gridPos.x >= board.columns || gridPos.y < 0 || gridPos.y >= board.rows) return;

        var content = board.GetCellContent(gridPos);
        bool isMatchingEndpoint = false;

        if (content == BoardManager.CellContent.Dot)
        {
            Color32 cellColorKey = board.GetDotColorAt(gridPos);
            if (!cellColorKey.Equals(currentPath.colorKey))
            {
                // Different colored dot sitting right there - just don't move, stay put
                return;
            }
            if (gridPos != currentPath.cells[0]) isMatchingEndpoint = true;
        }

        // Backtracking: if the player drags back over the previous cell, remove the last segment
        if (currentPath.cells.Count >= 2 && gridPos == currentPath.cells[currentPath.cells.Count - 2])
        {
            RemoveLastSegment();
            return;
        }

        // Cell already used by our own current path - stay put
        if (currentPath.cells.Contains(gridPos))
        {
            return;
        }

        // Cell already used by a different color's committed path - stay put
        foreach (var kvp in committedPaths)
        {
            if (!kvp.Value.colorKey.Equals(currentPath.colorKey) && kvp.Value.cells.Contains(gridPos))
            {
                Debug.Log($"Blocked - {gridPos} already used by pair {kvp.Key}");
                return;
            }
        }

        AddSegment(gridPos);

        if (isMatchingEndpoint)
        {
            Debug.Log("Path reached a matching-color dot - completing.");
            FinishPath();
        }
    }

    void AddSegment(Vector2Int newCell)
    {
        currentPath.cells.Add(newCell);
        Debug.Log($"Added segment at {newCell} - path length now {currentPath.cells.Count}");
        RebuildPathVisuals();
        UpdateDragIndicatorPosition();
    }

    void RemoveLastSegment()
    {
        Vector2Int removed = currentPath.cells[currentPath.cells.Count - 1];
        currentPath.cells.RemoveAt(currentPath.cells.Count - 1);
        Debug.Log($"Backtracked - removed {removed} - path length now {currentPath.cells.Count}");
        RebuildPathVisuals();
        UpdateDragIndicatorPosition();
    }

    void UpdateDragIndicatorPosition()
    {
        if (activeDragIndicator == null) return;

        Vector2Int currentEnd = currentPath.cells[currentPath.cells.Count - 1];
        activeDragIndicator.transform.position = board.GridToWorld(currentEnd);

        if (currentPath.cells.Count >= 2)
        {
            Vector2Int previous = currentPath.cells[currentPath.cells.Count - 2];
            Vector2Int direction = currentEnd - previous;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            activeDragIndicator.transform.rotation = Quaternion.Euler(0, 0, angle + dragIndicatorRotationOffset);
        }
    }

    // Clears and redraws every stem piece for the current path based on the cell list.
    // Simpler than trying to patch individual pieces, and fine for small grids.
    void RebuildPathVisuals()
    {
        foreach (var obj in currentPath.segmentObjects)
        {
            Destroy(obj);
        }
        currentPath.segmentObjects.Clear();

        for (int i = 1; i < currentPath.cells.Count - 1; i++)
        {
            Vector2Int prev = currentPath.cells[i - 1];
            Vector2Int cur = currentPath.cells[i];
            Vector2Int next = currentPath.cells[i + 1];

            Vector2Int dirIn = cur - prev;
            Vector2Int dirOut = next - cur;

            GameObject prefabToUse;
            float rotationZ;

            if (dirIn == dirOut)
            {
                prefabToUse = straightPrefab;
                rotationZ = (dirIn.x != 0) ? 90f : 0f;
                rotationZ += straightRotationOffset;
            }
            else
            {
                prefabToUse = cornerPrefab;
                rotationZ = GetCornerRotation(dirIn, dirOut);
                rotationZ += cornerRotationOffset;
            }

            Vector3 worldPos = board.GridToWorld(cur);
            GameObject segment = Instantiate(prefabToUse, worldPos, Quaternion.Euler(0, 0, rotationZ), transform);
            SpriteRenderer sr = segment.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = currentPath.color;

            string pieceType = (dirIn == dirOut) ? "straight" : "corner";
            Debug.Log($"Placed {pieceType} at {cur}, rotation {rotationZ}");

            currentPath.segmentObjects.Add(segment);
        }
    }

    // The corner sprite's un-rotated (0 degree) art connects the LEFT and BOTTOM edges.
    // Each entry below lists both directions of travel that produce the same physical connection.
    float GetCornerRotation(Vector2Int dirIn, Vector2Int dirOut)
    {
        if ((dirIn == Vector2Int.right && dirOut == Vector2Int.down) ||
            (dirIn == Vector2Int.up && dirOut == Vector2Int.left))
            return 0f;

        if ((dirIn == Vector2Int.up && dirOut == Vector2Int.right) ||
            (dirIn == Vector2Int.left && dirOut == Vector2Int.down))
            return 90f;

        if ((dirIn == Vector2Int.left && dirOut == Vector2Int.up) ||
            (dirIn == Vector2Int.down && dirOut == Vector2Int.right))
            return 180f;

        if ((dirIn == Vector2Int.right && dirOut == Vector2Int.up) ||
            (dirIn == Vector2Int.down && dirOut == Vector2Int.left))
            return 270f;

        return 0f;
    }

    void EndDrag()
    {
        Debug.Log($"Drag ended - final path had {currentPath.cells.Count} cells");
        FinishPath();
    }

    // Shared by both releasing the mouse and auto-completing when a matching dot is reached
    void FinishPath()
    {
        isDragging = false;

        if (activeDragIndicator != null)
        {
            Destroy(activeDragIndicator);
            activeDragIndicator = null;
        }

        Vector2Int startCell = currentPath.cells[0];
        Vector2Int endCell = currentPath.cells[currentPath.cells.Count - 1];

        bool connected = false;
        if (currentPath.cells.Count >= 2 && endCell != startCell &&
            board.GetCellContent(endCell) == BoardManager.CellContent.Dot)
        {
            Color32 endColor = board.GetDotColorAt(endCell);
            connected = endColor.Equals(currentPath.colorKey);
        }

        committedPaths[currentPath.pairIndex] = new CommittedPath
        {
            colorKey = currentPath.colorKey,
            cells = new List<Vector2Int>(currentPath.cells),
            visuals = currentPath.segmentObjects,
            connected = connected
        };

        currentPath = null;

        if (!won && AllDotsConnected())
        {
            won = true;
            if (minigameEnd != null) minigameEnd.Win();
        }
    }
    bool AllDotsConnected()
    {
        HashSet<Vector2Int> connectedDots = new HashSet<Vector2Int>();

        foreach (var kvp in committedPaths)
        {
            CommittedPath p = kvp.Value;
            if (!p.connected) continue;
            connectedDots.Add(p.cells[0]);
            connectedDots.Add(p.cells[p.cells.Count - 1]);
        }

        if (connectedDots.Count == 0) return false;

        for (int x = 0; x < board.columns; x++)
        {
            for (int y = 0; y < board.rows; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (board.GetCellContent(pos) == BoardManager.CellContent.Dot &&
                    !connectedDots.Contains(pos))
                    return false;
            }
        }

        return true;
    }
}