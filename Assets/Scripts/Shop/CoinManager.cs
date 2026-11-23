using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public TextMeshProUGUI[] moneyTexts;

    void Start()
    {
        if (Inventory.Instance == null)
        {
            Debug.LogError("Inventory not found! Start from the scene that has it.");
            return;
        }

        Inventory.Instance.OnMoneyChanged += UpdateMoneyText;
        UpdateMoneyText();
    }

    void OnDestroy()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnMoneyChanged -= UpdateMoneyText;
    }

    void UpdateMoneyText()
    {
        string formattedCoins = Inventory.Instance.money.ToString("N0");

        foreach (TextMeshProUGUI moneyText in moneyTexts)
        {
            moneyText.text = formattedCoins;
        }
    }
}