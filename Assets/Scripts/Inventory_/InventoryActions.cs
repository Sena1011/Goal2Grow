using UnityEngine;

public class InventoryActions : MonoBehaviour
{
    public void AddOneGrowth()
    {
        if (Inventory.Instance != null) Inventory.Instance.AddGrowth(1);
    }

    public void ResetGrowth()
    {
        if (Inventory.Instance != null) Inventory.Instance.ResetGrowth();
    }
}