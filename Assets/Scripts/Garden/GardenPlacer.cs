using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GardenPlacer : MonoBehaviour
{
    public GardenGrid grid;
    public Transform placedParent;
    public ItemData selected;
    public System.Action onPlaced;   // also fired after a removal, so the menu refreshes its counts

    [Header("Preview and confirm")]
    public GameObject confirmPanel;  // Place + Cancel buttons (starts hidden)
    public Color validColor = new Color(0.55f, 1f, 0.55f, 0.65f);
    public Color invalidColor = new Color(1f, 0.4f, 0.4f, 0.65f);

    [Header("Remove")]
    public GameObject removePanel;   // Remove + Keep buttons (starts hidden)
    public Color selectedTint = new Color(1f, 1f, 0.55f, 1f);
    public GameObject customizeMenu; // optional: if set, items can only be selected for removal while this is open
    public TMP_Text messageText;     // optional: short messages like "Take the flower out first"
    public float panelGap = 20f;   // space between the item and its buttons, in canvas units

    readonly Dictionary<PlacedItem, SpriteRenderer> spawned = new Dictionary<PlacedItem, SpriteRenderer>();

    GameObject ghost;
    SpriteRenderer ghostSr;
    Vector2Int pendingCell;
    bool hasPending, pendingValid;
    PlacedItem selectedPlaced;

    float ScaleFor(ItemData item)
    {
        float s = item.displayScale > 0f ? item.displayScale : 1f;
        if (item.fitToFootprint && item.sprite != null)
            s *= (item.size.x * grid.cellSize) / item.sprite.bounds.size.x;   // fill the footprint's width
        return s;
    }

    void Start()
    {
        var inv = Inventory.Instance;
        if (inv == null) return;

        // return any seeds that were placed by mistake to the owned items
        var wrongly = inv.placedItems.FindAll(p => p.item != null && p.item.isSeed);
        foreach (var p in wrongly) inv.ownedItems.Add(p.item);
        inv.placedItems.RemoveAll(p => p.item != null && p.item.isSeed);
        if (wrongly.Count > 0) inv.Save();

        foreach (var p in inv.placedItems) Spawn(p);

        if (confirmPanel != null) confirmPanel.SetActive(false);
        if (removePanel != null) removePanel.SetActive(false);
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    // ---------- Choosing what to place ----------

    public void Select(ItemData item)
    {
        if (item != null && item.isSeed) return;   // seeds are planted in the homeroom
        ClearPending();
        Deselect();
        selected = item;
    }

    // ---------- Taps ----------

    void Update()
    {
        if (!Input.GetMouseButtonDown(0) || OverUI()) return;
        if (selected != null) HandlePlaceTap(); else HandleRemoveTap();
    }

    bool TryGetTapCell(out Vector2Int cell)
    {
        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cell = grid.WorldToCell(world);
        return grid.InBounds(cell);
    }

    // ---------- Placing (preview, then confirm) ----------

    void HandlePlaceTap()
    {
        if (!TryGetTapCell(out Vector2Int cell)) return;

        if (Inventory.Instance.GetCount(selected) <= 0) { selected = null; ClearPending(); return; }

        pendingCell = cell;
        hasPending = true;
        pendingValid = CanPlace(selected, cell);

        ShowGhost();
        if (confirmPanel != null) confirmPanel.SetActive(pendingValid);   // the Place button only appears on a valid spot
        if (!pendingValid) ShowMessage("Can't place that here");
    }

    public void ConfirmPlacement()   // hook to the Place button
    {
        if (selected == null || !hasPending || !pendingValid) return;

        var inv = Inventory.Instance;
        if (inv.GetCount(selected) <= 0) { selected = null; ClearPending(); return; }

        inv.Place(selected, pendingCell);
        Spawn(inv.placedItems[inv.placedItems.Count - 1]);
        onPlaced?.Invoke();

        ClearPending();
        if (inv.GetCount(selected) <= 0) selected = null;
    }

    public void CancelPlacement()    // hook to the Cancel button
    {
        ClearPending();
    }

    void ShowGhost()
    {
        if (ghost == null)
        {
            ghost = new GameObject("PlacementPreview");
            ghostSr = ghost.AddComponent<SpriteRenderer>();
            ghostSr.sortingOrder = 5000;   // above everything
        }
        ghostSr.sprite = selected.sprite;
        ghostSr.color = pendingValid ? validColor : invalidColor;
        ghost.transform.position = CellCenter(selected, pendingCell);
        ghost.transform.localScale = Vector3.one * ScaleFor(selected);
        ghost.SetActive(true);
    }

    void ClearPending()
    {
        hasPending = false;
        if (ghost != null) ghost.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    // ---------- Removing ----------

    void HandleRemoveTap()
    {
        if (customizeMenu != null && !customizeMenu.activeInHierarchy) { Deselect(); return; }

        if (!TryGetTapCell(out Vector2Int cell)) { Deselect(); return; }

        PlacedItem hit = PlantAt(cell) ?? GroundItemAt(cell);   // a flower is picked before the pot under it
        if (hit == null) { Deselect(); return; }

        SelectPlaced(hit);
    }

    void SelectPlaced(PlacedItem p)
    {
        Deselect();
        selectedPlaced = p;
        if (spawned.TryGetValue(p, out var sr) && sr != null) sr.color = selectedTint;
        if (removePanel != null) removePanel.SetActive(true);
    }

    public void Deselect()           // hook to the Keep button
    {
        if (selectedPlaced != null && spawned.TryGetValue(selectedPlaced, out var sr) && sr != null)
            sr.color = Color.white;
        selectedPlaced = null;
        if (removePanel != null) removePanel.SetActive(false);
    }

    public void RemoveSelected()     // hook to the Remove button
    {
        if (selectedPlaced == null) return;

        if (selectedPlaced.item.isPlantBox && HasPlantOn(selectedPlaced))
        {
            ShowMessage("Take the flower out first");
            return;
        }

        PlacedItem p = selectedPlaced;
        Deselect();

        if (spawned.TryGetValue(p, out var sr))
        {
            if (sr != null) Destroy(sr.gameObject);
            spawned.Remove(p);
        }

        Inventory.Instance.RemovePlaced(p);
        onPlaced?.Invoke();
    }

    bool HasPlantOn(PlacedItem ground)
    {
        for (int x = 0; x < ground.item.size.x; x++)
            for (int y = 0; y < ground.item.size.y; y++)
                if (PlantAt(ground.cell + new Vector2Int(x, y)) != null) return true;
        return false;
    }

    // ---------- Grid rules ----------

    PlacedItem GroundItemAt(Vector2Int cell)
    {
        foreach (var p in Inventory.Instance.placedItems)
            if (!p.item.needsPlantBox && Covers(p, cell)) return p;
        return null;
    }

    PlacedItem PlantAt(Vector2Int cell)
    {
        foreach (var p in Inventory.Instance.placedItems)
            if (p.item.needsPlantBox && p.cell == cell) return p;
        return null;
    }

    bool Covers(PlacedItem p, Vector2Int cell)
    {
        return cell.x >= p.cell.x && cell.x < p.cell.x + p.item.size.x &&
               cell.y >= p.cell.y && cell.y < p.cell.y + p.item.size.y;
    }

    bool CanPlace(ItemData item, Vector2Int cell)
    {
        if (item.isSeed) return false;

        if (item.needsPlantBox)
        {
            var ground = GroundItemAt(cell);
            return ground != null && ground.item.isPlantBox && PlantAt(cell) == null;
        }
        for (int x = 0; x < item.size.x; x++)
            for (int y = 0; y < item.size.y; y++)
            {
                var c = cell + new Vector2Int(x, y);
                if (!grid.InBounds(c) || GroundItemAt(c) != null) return false;
            }
        return true;
    }

    // ---------- Drawing ----------

    Vector3 CellCenter(ItemData item, Vector2Int cell)
    {
        Vector3 pos = grid.CellToWorld(cell) + new Vector3(
            (item.size.x - 1) * 0.5f * grid.cellSize,
            (item.size.y - 1) * 0.5f * grid.cellSize, 0);

        pos += new Vector3(item.displayOffset.x, item.displayOffset.y, 0f);

        if (item.fitToFootprint && item.sprite != null)
        {
            float areaH = item.size.y * grid.cellSize;
            float spriteH = item.sprite.bounds.size.y * ScaleFor(item);
            pos.y += (spriteH - areaH) * 0.5f;   // tall pictures grow upward, with their base on the ground
        }
        return pos;
    }

    void Spawn(PlacedItem p)
    {
        var item = p.item;
        var go = new GameObject(item.displayName);
        go.transform.SetParent(placedParent);
        go.transform.position = CellCenter(item, p.cell);
        go.transform.localScale = Vector3.one * ScaleFor(item);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = item.sprite;
        sr.sortingOrder = (item.needsPlantBox ? 1000 : 0) + 100 - p.cell.y;
        // if you added the Fit To Footprint block earlier, paste it here (it uses `item`, `sr` and `go`)
        spawned[p] = sr;
    }

    // ---------- Small helpers ----------

    void ShowMessage(string text)
    {
        if (messageText == null) return;
        messageText.text = text;
        messageText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideMessage));
        Invoke(nameof(HideMessage), 2f);
    }

    void HideMessage()
    {
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    static readonly List<RaycastResult> uiHits = new List<RaycastResult>();

    bool OverUI()
    {
        if (EventSystem.current == null) return false;

        Vector2 pos = Input.touchCount > 0 ? (Vector2)Input.GetTouch(0).position
                                           : (Vector2)Input.mousePosition;

        var data = new PointerEventData(EventSystem.current) { position = pos };
        uiHits.Clear();
        EventSystem.current.RaycastAll(data, uiHits);

        foreach (var h in uiHits)
            if (h.module is GraphicRaycaster) return true;   // only real UI counts, not physics colliders

        return false;
    }

    void LateUpdate()
    {
        // keep the button panels attached to their item
        if (hasPending && confirmPanel != null && confirmPanel.activeSelf && ghost != null && ghost.activeSelf)
            PositionPanel(confirmPanel, ghostSr.bounds);

        if (selectedPlaced != null && removePanel != null && removePanel.activeSelf &&
            spawned.TryGetValue(selectedPlaced, out var sr) && sr != null)
            PositionPanel(removePanel, sr.bounds);
    }

    void PositionPanel(GameObject panelObj, Bounds b)
    {
        var panel = panelObj.transform as RectTransform;
        var parent = panel != null ? panel.parent as RectTransform : null;
        var canvas = panel != null ? panel.GetComponentInParent<Canvas>() : null;
        if (parent == null || canvas == null || Camera.main == null) return;

        canvas = canvas.rootCanvas;
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        Vector2 sBottom = Camera.main.WorldToScreenPoint(new Vector3(b.center.x, b.min.y, b.center.z));
        Vector2 sTop    = Camera.main.WorldToScreenPoint(new Vector3(b.center.x, b.max.y, b.center.z));
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, sBottom, uiCam, out Vector2 lBottom);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, sTop, uiCam, out Vector2 lTop);

        float w = panel.rect.width * panel.localScale.x;
        float h = panel.rect.height * panel.localScale.y;
        Rect pr = parent.rect;

        // below the item if there's room, otherwise above it
        bool below = lBottom.y - panelGap - h >= pr.yMin;
        panel.pivot = new Vector2(0.5f, below ? 1f : 0f);
        Vector2 pos = below ? new Vector2(lBottom.x, lBottom.y - panelGap)
                            : new Vector2(lTop.x, lTop.y + panelGap);

        pos.x = Mathf.Clamp(pos.x, pr.xMin + w * 0.5f, pr.xMax - w * 0.5f);   // keep it on screen sideways
        panel.localPosition = new Vector3(pos.x, pos.y, 0f);
    }
}