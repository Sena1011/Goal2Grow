using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PlacedItem
{
    public ItemData item;          // the pot (or decor)
    public Vector2Int cell;

    public ItemData plant;         // null = empty pot
    public int plantStartGrowth;   // growthPoints when the seed was planted
}

[System.Serializable]
public class StoredPlant
{
    public ItemData plant;
}

// THIS SHIT WAS SO HARD TO FIGURE OUT IMA DIE!!!!
// ---- what gets written to the save file (items are saved by Id, not by reference) ----
[System.Serializable]
class SavedPlacedItem
{
    public string itemId;
    public int x;
    public int y;
    public string plantId;
    public int plantStartGrowth;
}

[System.Serializable]
class InventorySave
{
    public int money;
    public int growthPoints;
    public bool gaveStarter;
    public string homePlantId = "";
    public int homePlantStart;
    public bool homePlantBloomAwarded;
    public List<string> ownedIds = new List<string>();
    public List<SavedPlacedItem> placed = new List<SavedPlacedItem>();
    public List<string> storedPlantIds = new List<string>();
    public List<string> unlockedToggles = new List<string>();
    public List<string> activeToggles = new List<string>();

    
}

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    public int money;
    public event System.Action OnMoneyChanged;

    public int growthPoints;
    public event System.Action OnGrowthChanged;

    public ItemData starterBox;
    public List<ItemData> ownedItems = new List<ItemData>();
    public List<PlacedItem> placedItems = new List<PlacedItem>();
    public List<StoredPlant> storedPlants = new List<StoredPlant>();

    public List<string> unlockedToggles = new List<string>();   // toggle decor the player owns
    public List<string> activeToggles = new List<string>();     // ...and has switched on
    public event System.Action OnToggleChanged;

    public bool homePlantBloomAwarded;   // has this plant already given its badge?

    public ItemData homePlant;     // seed growing in the homeroom (null = nothing planted)
    public int homePlantStart;     // growthPoints when it was planted
    

    bool gaveStarter;
    Dictionary<string, ItemData> lookup;

    string SavePath => Path.Combine(Application.persistentDataPath, "inventory.json");

    public bool OwnsToggle(ItemData item) { return item != null && unlockedToggles.Contains(item.id); }
    public bool IsToggleOn(ItemData item) { return item != null && activeToggles.Contains(item.id); }

    public void SetToggle(ItemData item, bool on)
    {
        if (!OwnsToggle(item)) return;
        if (on && !activeToggles.Contains(item.id)) activeToggles.Add(item.id);
        if (!on) activeToggles.Remove(item.id);
        OnToggleChanged?.Invoke();
        Save();
    }

    public void RemovePlaced(PlacedItem p)
    {
        if (p == null || !placedItems.Remove(p)) return;
        if (p.item != null) ownedItems.Add(p.item);   // back into the player's inventory
        Save();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildLookup();

        if (!Load())
        {
            // first launch (no save yet)
            growthPoints = PlayerPrefs.GetInt("GrowthPoints", 0);
            if (starterBox != null) ownedItems.Add(starterBox);
            gaveStarter = true;
            Save();
        }

        Debug.Log("Inventory save file: " + SavePath);
    }

    // ---------- Saving and loading ----------

    void BuildLookup()
    {
        lookup = new Dictionary<string, ItemData>();
        foreach (ItemData item in Resources.LoadAll<ItemData>("Items"))
        {
            if (string.IsNullOrEmpty(item.id))
            {
                Debug.LogWarning("ItemData with no Id in Resources/Items: " + item.name);
                continue;
            }
            if (lookup.ContainsKey(item.id))
            {
                Debug.LogWarning("Two items share the Id '" + item.id + "' (" + item.name + "). Give each its own Id.");
                continue;
            }
            lookup[item.id] = item;
        }
    }

    ItemData FindItem(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (lookup.TryGetValue(id, out ItemData item)) return item;
        Debug.LogWarning("Save file mentions an item Id that isn't in Resources/Items: " + id);
        return null;
    }

    // lowkey had to consult claude for this save method cause i didnt know how it works.
    public void Save()
    {
        if (Instance != this) return;

        InventorySave d = new InventorySave();
        d.money = money;
        d.growthPoints = growthPoints;
        d.gaveStarter = gaveStarter;
        d.homePlantId = homePlant != null ? homePlant.id : "";
        d.homePlantStart = homePlantStart;
        d.homePlantBloomAwarded = homePlantBloomAwarded;

        foreach (ItemData i in ownedItems)
            if (i != null) d.ownedIds.Add(i.id);

        foreach (PlacedItem p in placedItems)
        {
            if (p.item == null) continue;
            d.placed.Add(new SavedPlacedItem
            {
                itemId = p.item.id,
                x = p.cell.x,
                y = p.cell.y,
                plantId = p.plant != null ? p.plant.id : "",
                plantStartGrowth = p.plantStartGrowth
            });
        }

        foreach (StoredPlant s in storedPlants)
            if (s.plant != null) d.storedPlantIds.Add(s.plant.id);

        d.unlockedToggles = new List<string>(unlockedToggles);
        d.activeToggles = new List<string>(activeToggles);

        File.WriteAllText(SavePath, JsonUtility.ToJson(d));
    }

    bool Load()
    {
        if (!File.Exists(SavePath)) return false;

        try
        {
            InventorySave d = JsonUtility.FromJson<InventorySave>(File.ReadAllText(SavePath));
            if (d == null) return false;

            money = d.money;
            growthPoints = d.growthPoints;
            gaveStarter = d.gaveStarter;
            homePlant = FindItem(d.homePlantId);
            homePlantStart = d.homePlantStart;
            homePlantBloomAwarded = d.homePlantBloomAwarded;
            unlockedToggles = d.unlockedToggles ?? new List<string>();
            activeToggles = d.activeToggles ?? new List<string>();

            ownedItems.Clear();
            foreach (string id in d.ownedIds)
            {
                ItemData item = FindItem(id);
                if (item != null) ownedItems.Add(item);
            }

            placedItems.Clear();
            foreach (SavedPlacedItem s in d.placed)
            {
                ItemData item = FindItem(s.itemId);
                if (item == null) continue;
                placedItems.Add(new PlacedItem
                {
                    item = item,
                    cell = new Vector2Int(s.x, s.y),
                    plant = FindItem(s.plantId),
                    plantStartGrowth = s.plantStartGrowth
                });
            }

            storedPlants.Clear();
            foreach (string id in d.storedPlantIds)
            {
                ItemData item = FindItem(id);
                if (item != null) storedPlants.Add(new StoredPlant { plant = item });
            }

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not read the inventory save: " + e.Message);
            return false;
        }
    }

    void OnApplicationPause(bool paused) { if (paused) Save(); }
    void OnApplicationQuit() { Save(); }

    [ContextMenu("Save now")]
    void SaveNow() { Save(); }

    [ContextMenu("Delete save file")]
    void DeleteSaveFile()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
        Debug.Log("Inventory save deleted. Stop and start the game for a fresh start.");
    }

    // ---------- Items ----------

    public int GetCount(ItemData item)
    {
        return ownedItems.FindAll(i => i == item).Count;
    }

    public bool TryBuy(ItemData item)
    {
        if (item.isToggleDecor)
        {
            if (OwnsToggle(item)) return false;       // can only be bought once
            if (money < item.price) return false;
            money -= item.price;
            unlockedToggles.Add(item.id);
            activeToggles.Add(item.id);               // starts switched on
            OnMoneyChanged?.Invoke();
            OnToggleChanged?.Invoke();
            Save();
            return true;
        }

        if (money < item.price) return false;
        money -= item.price;
        ownedItems.Add(item);
        OnMoneyChanged?.Invoke();
        Save();
        return true;
    }

    public void AddMoney(int amount)
    {
        money = Mathf.Max(0, money + amount);
        OnMoneyChanged?.Invoke();
        Save();
    }

    public void Place(ItemData item, Vector2Int cell)
    {
        if (ownedItems.Remove(item))
        {
            placedItems.Add(new PlacedItem { item = item, cell = cell });
            Save();
        }
    }

    // ---------- Plants ----------
    //i picked this course cause i thought "making apps seems fun" i was mistaken.

    public int GetPlantGrowth(PlacedItem pot)
    {
        if (pot.plant == null) return 0;
        return Mathf.Max(0, growthPoints - pot.plantStartGrowth);
    }

    public bool IsFullyGrown(PlacedItem pot)
    {
        return pot.plant != null && GetPlantGrowth(pot) >= pot.plant.growthNeeded;
    }

    public int GetHomeGrowth()
    {
        if (homePlant == null) return 0;
        return Mathf.Max(0, growthPoints - homePlantStart);
    }

    public bool HomePlantFullyGrown()
    {
        return homePlant != null && GetHomeGrowth() >= homePlant.growthNeeded;
    }

    public bool PlantHomeSeed(ItemData seed)
    {
        if (homePlant != null) return false;              // already growing something
        if (seed == null || !seed.isSeed) return false;
        if (!ownedItems.Remove(seed)) return false;       // you don't own it
        homePlant = seed;
        homePlantBloomAwarded = false;
        homePlantStart = growthPoints;
        OnGrowthChanged?.Invoke();
        Save();
        return true;
    }

    public bool StoreHomePlant()
    {
        if (!HomePlantFullyGrown()) return false;
        if (homePlant.grownItem != null) ownedItems.Add(homePlant.grownItem);
        storedPlants.Add(new StoredPlant { plant = homePlant });
        homePlantBloomAwarded = false;
        homePlant = null;                                 // homeroom is free for a new seed
        OnGrowthChanged?.Invoke();
        Save();
        return true;
    }

    void CheckBloom()
    {
        if (homePlant == null || homePlantBloomAwarded) return;
        if (!HomePlantFullyGrown()) return;

        homePlantBloomAwarded = true;   // so it only counts once per plant
        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.AddFlower(homePlant.id);
    }

    // ---------- Growth ----------
    // for something that only has a few lines this shit was probably the main reason this game revolves around.

    public void AddGrowth(int amount)
    {
        growthPoints = Mathf.Max(0, growthPoints + amount);
        PlayerPrefs.SetInt("GrowthPoints", growthPoints);
        PlayerPrefs.Save();
        CheckBloom();
        OnGrowthChanged?.Invoke();
        Save();
    }

    public void ResetGrowth()
    {
        growthPoints = 0;
        homePlantStart = 0;     // keeps the plant from going negative
        homePlantBloomAwarded = false;
        PlayerPrefs.SetInt("GrowthPoints", 0);
        PlayerPrefs.Save();
        OnGrowthChanged?.Invoke();
        Save();
    }

    public void ResetAll()
    {
        money = 0;
        ownedItems.Clear();
        placedItems.Clear();
        storedPlants.Clear();
        homePlantBloomAwarded = false;
        homePlant = null;
        homePlantStart = 0;
        growthPoints = 0;
        unlockedToggles.Clear();
        activeToggles.Clear();
        PlayerPrefs.DeleteKey("GrowthPoints");   // only this key, so volume settings survive
        PlayerPrefs.Save();

        if (File.Exists(SavePath)) File.Delete(SavePath);
        if (starterBox != null) ownedItems.Add(starterBox);
        gaveStarter = true;

        OnMoneyChanged?.Invoke();
        OnGrowthChanged?.Invoke();
        Save();
    }
}