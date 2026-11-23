using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryRow : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text countText;
    public Button button;

    public void Setup(ItemData item, int count, System.Action onClick)
    {
        icon.sprite = item.sprite;
        nameText.text = item.displayName;
        countText.text = "x" + count;

        button.onClick.RemoveAllListeners();
        if (onClick != null)
        {
            button.interactable = true;
            button.onClick.AddListener(() => onClick());
        }
        else
        {
            button.interactable = false;   // flowers are display-only
        }
    }
}