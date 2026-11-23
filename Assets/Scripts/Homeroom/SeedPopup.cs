using System.Collections.Generic;
using UnityEngine;

public class SeedPopup : MonoBehaviour
{
    public GameObject panel;
    public Transform seedsList;
    public Transform flowersList;
    public InventoryRow rowPrefab;
    public Collider2D[] blockWhileOpen;   // laptop, window, door, task note...

    void Start()
    {
        panel.SetActive(false);
    }

    public void Open()
    {
        panel.SetActive(true);
        SetColliders(false);
        Rebuild();
    }

    public void Close()
    {
        panel.SetActive(false);
        SetColliders(true);
    }

    void SetColliders(bool on)
    {
        foreach (var c in blockWhileOpen)
            if (c != null) c.enabled = on;
    }

    void Rebuild()
    {
        var inv = Inventory.Instance;
        if (inv == null) return;

        Clear(seedsList);
        Clear(flowersList);

        var shown = new List<ItemData>();
        foreach (var item in inv.ownedItems)
        {
            if (shown.Contains(item)) continue;   // one row per item type
            shown.Add(item);
            int count = inv.GetCount(item);

            if (item.isSeed)
            {
                var row = Instantiate(rowPrefab, seedsList);
                var seed = item;
                row.Setup(seed, count, () =>
                {
                    if (inv.PlantHomeSeed(seed)) Close();
                });
            }
            else if (item.needsPlantBox)   // your flowers
            {
                var row = Instantiate(rowPrefab, flowersList);
                row.Setup(item, count, null);
            }
        }
    }

    void Clear(Transform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }
}