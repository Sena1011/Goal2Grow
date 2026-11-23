using UnityEngine;

public enum ItemCategory { FlowersAndSeed, GardenDecor, PotsAndSoil }

[CreateAssetMenu(menuName = "Garden/Item")]
public class ItemData : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite sprite;
    public int price;
    public ItemCategory category;
    public Vector2Int size = Vector2Int.one;
    public bool isPlantBox;
    public bool needsPlantBox;
    public bool isSeed;
    public int growthNeeded;
    public ItemData grownItem;
    public Sprite[] stageSprites;
    public float displayScale = 1f;   // 1 = natural size, 1.5 = half again as big in the garden
    public Vector2 displayOffset;   // nudges the picture in the garden (world units). Negative Y = lower
    
    public bool fitToFootprint;       // stretch the picture across its cells (good for decor)
    public bool isToggleDecor;   // tick for the fence: bought once, switched on/off, never placed on cells
}