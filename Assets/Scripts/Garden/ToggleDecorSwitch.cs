using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleDecorSwitch : MonoBehaviour
{
    public ItemData item;          // the Fence asset
    public GameObject showWhenOwned;   // optional: the row or label to hide until the fence is bought

    Toggle toggle;

    void Awake() { toggle = GetComponent<Toggle>(); }

    void OnEnable()
    {
        Refresh();
        toggle.onValueChanged.AddListener(OnChanged);
    }

    void OnDisable()
    {
        toggle.onValueChanged.RemoveListener(OnChanged);
    }

    void Refresh()
    {
        var inv = Inventory.Instance;
        bool owned = inv != null && inv.OwnsToggle(item);
        if (showWhenOwned != null) showWhenOwned.SetActive(owned);
        toggle.SetIsOnWithoutNotify(owned && inv.IsToggleOn(item));
    }

    void OnChanged(bool on)
    {
        if (Inventory.Instance != null) Inventory.Instance.SetToggle(item, on);
    }
}