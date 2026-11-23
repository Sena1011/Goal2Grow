using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public int id;
    public Image cardImage;
    public Sprite cardBack;
    public Sprite cardFace;

    public bool isFlipped = false;
    public bool isMatched = false;

    private MemoryGameManager gameManager;

    void Awake()
    {
        // Grab the Image component automatically if not assigned
        if (cardImage == null)
            cardImage = GetComponent<Image>();
    }

    void Start()
    {
        gameManager = FindAnyObjectByType<MemoryGameManager>();
        cardImage.sprite = cardBack;
    }

    public void OnCardClicked()
    {
        if (isFlipped || isMatched || !gameManager.CanFlip()) return;
        FlipUp();
        gameManager.CardSelected(this);
    }

    public void FlipUp()
    {
        isFlipped = true;
        cardImage.sprite = cardFace;
    }

    public void FlipDown()
    {
        isFlipped = false;
        cardImage.sprite = cardBack;
    }

    public void SetMatched()
    {
        isMatched = true;
    }
}