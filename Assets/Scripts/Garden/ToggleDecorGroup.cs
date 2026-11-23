using UnityEngine;

public class ToggleDecorGroup : MonoBehaviour
{
    public ItemData item;        // the Fence asset
    public GameObject visual;    // the fence pieces, a child of this object

    void OnEnable()
    {
        if (Inventory.Instance != null) Inventory.Instance.OnToggleChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (Inventory.Instance != null) Inventory.Instance.OnToggleChanged -= Refresh;
    }

    void Refresh()
    {
        var inv = Inventory.Instance;
        if (inv == null || visual == null) return;
        visual.SetActive(inv.OwnsToggle(item) && inv.IsToggleOn(item));
    }
}