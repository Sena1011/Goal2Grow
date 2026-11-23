using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public TextMeshProUGUI shopTitle;

    public GameObject flowerSeedsPanel;
    public GameObject flowerPotsPanel;
    public GameObject soilPanel;
    public GameObject gardenDecorPanel;

    void Start()
    {
        ShowFlowerSeeds();
    }

    public void ShowFlowerSeeds() { ShowOnly(flowerSeedsPanel, "Flower Seeds"); }
    public void ShowFlowerPots()  { ShowOnly(flowerPotsPanel, "Flower Pots"); }
    public void ShowSoil()        { ShowOnly(soilPanel, "Soil"); }
    public void ShowGardenDecor() { ShowOnly(gardenDecorPanel, "Garden Decor"); }

    void ShowOnly(GameObject target, string title)
    {
        if (flowerSeedsPanel != null) flowerSeedsPanel.SetActive(false);
        if (flowerPotsPanel != null) flowerPotsPanel.SetActive(false);
        if (soilPanel != null) soilPanel.SetActive(false);
        if (gardenDecorPanel != null) gardenDecorPanel.SetActive(false);

        if (target != null) target.SetActive(true);
        if (shopTitle != null) shopTitle.text = title;
    }
}