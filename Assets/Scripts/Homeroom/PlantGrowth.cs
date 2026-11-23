using UnityEngine;

public class PlantGrowth : MonoBehaviour
{
    public GameObject plantButton;   // shown when nothing is planted
    public GameObject storeButton;   // shown when fully grown

    SpriteRenderer sr;

    // 0 = just planted, 1 = fully grown (use this for your progress bar)
    public float Progress
    {
        get
        {
            var inv = Inventory.Instance;
            if (inv == null || inv.homePlant == null || inv.homePlant.growthNeeded <= 0) return 0f;
            return Mathf.Clamp01((float)inv.GetHomeGrowth() / inv.homePlant.growthNeeded);
        }
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnGrowthChanged += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnGrowthChanged -= Refresh;
    }

    void Refresh()
    {
        var inv = Inventory.Instance;
        if (inv == null) return;

        var seed = inv.homePlant;
        bool hasPlant = seed != null && seed.stageSprites != null && seed.stageSprites.Length > 0;

        sr.enabled = hasPlant;
        if (hasPlant)
        {
            int n = seed.stageSprites.Length;
            int stage = Mathf.Min((int)(Progress * (n - 1)), n - 1);
            sr.sprite = seed.stageSprites[stage];
        }

        if (plantButton != null) plantButton.SetActive(seed == null);
        if (storeButton != null) storeButton.SetActive(inv.HomePlantFullyGrown());
    }

    // Hook these to the buttons' OnClick
    public void PlantFirstSeed()
    {
        var inv = Inventory.Instance;
        if (inv == null) return;

        var seed = inv.ownedItems.Find(i => i.isSeed);
        if (seed == null) { Debug.Log("No seeds owned. Buy one in the shop."); return; }

        inv.PlantHomeSeed(seed);
    }

    public void StorePlant()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.StoreHomePlant();
    }
}