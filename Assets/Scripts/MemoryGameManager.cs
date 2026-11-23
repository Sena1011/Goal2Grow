using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MemoryGameManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform gridParent;
    public Sprite[] faceSprites; // one sprite per unique pair
    public Sprite cardBack;

    [Header("Reward")]
    public int rewardCoins = 50;

    [Header("Win Panel")]
    public GameObject winPanel;
    public TMP_Text winText;
    public string homeSceneName = "HomeRoom";   // must match your scene's name exactly

    private Card firstCard, secondCard;
    private bool canFlip = true;
    private int matchesFound = 0;
    private int totalPairs;

    void Start()
    {
        totalPairs = faceSprites.Length;

        if (winPanel != null)
            winPanel.SetActive(false);

        SpawnCards();
    }

    void SpawnCards()
    {
        List<int> ids = new List<int>();
        for (int i = 0; i < faceSprites.Length; i++)
        {
            ids.Add(i);
            ids.Add(i); // each id appears twice (a pair)
        }

        // Shuffle the list (Fisher-Yates shuffle)
        for (int i = 0; i < ids.Count; i++)
        {
            int temp = ids[i];
            int randIndex = Random.Range(i, ids.Count);
            ids[i] = ids[randIndex];
            ids[randIndex] = temp;
        }

        foreach (int id in ids)
        {
            GameObject cardObj = Instantiate(cardPrefab, gridParent);
            Card card = cardObj.GetComponent<Card>();
            card.id = id;
            card.cardFace = faceSprites[id];
            card.cardBack = cardBack;
        }
    }

    public bool CanFlip()
    {
        return canFlip;
    }

    public void CardSelected(Card card)
    {
        if (firstCard == null)
        {
            firstCard = card;
        }
        else if (secondCard == null)
        {
            secondCard = card;
            canFlip = false;
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(0.6f); // pause so player can see both cards

        if (firstCard.id == secondCard.id)
        {
            firstCard.SetMatched();
            secondCard.SetMatched();
            matchesFound++;

            if (matchesFound >= totalPairs)
                Win();
        }
        else
        {
            firstCard.FlipDown();
            secondCard.FlipDown();
        }

        firstCard = null;
        secondCard = null;
        canFlip = true;
    }

    void Win()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.AddMoney(rewardCoins);

        if (winText != null)
            winText.text = "You won!\n+" + rewardCoins + " coins";

        if (winPanel != null)
            winPanel.SetActive(true);
    }

    public void RestartGame()
    {
        StopAllCoroutines();   // cancels a half-finished match check

            foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        firstCard = null;
        secondCard = null;
        canFlip = true;
        matchesFound = 0;

        if (winPanel != null)
            winPanel.SetActive(false);

        SpawnCards();
    }

    public void GoHome()
    {
        SceneManager.LoadScene(homeSceneName);
    }
}