using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizationPanel : MonoBehaviour
{
    public GardenPlacer placer;
    public ItemSlot slotPrefab;
    public GameObject[] pages;
    public Transform[] contents;

    public Image[] tabImages;
    public Color activeColor = new Color32(107, 155, 107, 255);
    public Color inactiveColor = new Color32(237, 228, 211, 255);

    void Start()
    {
        placer.onPlaced += Refresh;
        Refresh();
        ShowPage(0);
    }

    void OnDestroy()
    {
        if (placer != null) placer.onPlaced -= Refresh;
    }

    public void ShowPage(int index)
    {
        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(i == index);

        for (int i = 0; i < tabImages.Length; i++)
            if (tabImages[i] != null)
                tabImages[i].color = (i == index) ? activeColor : inactiveColor;
    }

    public void Refresh()
    {
        for (int i = 0; i < contents.Length; i++)
        {
            foreach (Transform child in contents[i])
                Destroy(child.gameObject);

            var shown = new List<ItemData>();
            foreach (var item in Inventory.Instance.ownedItems)
            {
                if ((int)item.category != i || shown.Contains(item)) continue;
                shown.Add(item);
                var slot = Instantiate(slotPrefab, contents[i]);
                slot.Setup(item, Inventory.Instance.GetCount(item), placer.Select);
            }
        }
    }
}