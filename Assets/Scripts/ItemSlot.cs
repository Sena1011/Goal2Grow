using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text countText;
    public Button button;

    public void Setup(ItemData item, int count, Action<ItemData> onClick)
    {
        icon.sprite = item.sprite;
        nameText.text = item.displayName;
        countText.text = count > 1 ? "x" + count : "";
        button.onClick.AddListener(() => onClick(item));
    }
}