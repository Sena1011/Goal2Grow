using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public ItemData item;   // drag the item asset in the Inspector

    public void BuyItem()
    {
        if (Inventory.Instance == null)
        {
            Debug.LogError("Inventory not found! Start from the scene that has it.");
            return;
        }

        if (item == null)
        {
            Debug.LogError("No ItemData assigned on " + gameObject.name);
            return;
        }

        if (Inventory.Instance.TryBuy(item))
        {
            Debug.Log("Purchased: " + item.name);
            // trigger the door/gift package animation here
        }
        else
        {
            Debug.Log("Not enough coins!");
        }
    }
}