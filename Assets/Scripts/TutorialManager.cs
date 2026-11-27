using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("Slides (one GameObject per slide)")]
    public GameObject[] slides;

    [Header("UI")]
    public Button prevButton;
    public Button nextButton;
    public Button closeButton;
    public TMP_Text pageText;

    [Header("What happens when Close is pressed")]
    public UnityEvent onClose;

    int current = 0;

    void Start()
    {
        prevButton.onClick.AddListener(Prev);
        nextButton.onClick.AddListener(Next);
        closeButton.onClick.AddListener(() => onClose.Invoke());
        Show(0);
    }

    // Each time the popup is opened, start from slide 1 again
    void OnEnable()
    {
        if (slides != null && slides.Length > 0 && prevButton != null)
            Show(0);
    }

    void Next() { if (current < slides.Length - 1) Show(current + 1); }
    void Prev() { if (current > 0) Show(current - 1); }

    void Show(int index)
    {
        current = index;
        for (int i = 0; i < slides.Length; i++)
            slides[i].SetActive(i == current);

        prevButton.gameObject.SetActive(current > 0);
        nextButton.gameObject.SetActive(current < slides.Length - 1);
        pageText.text = (current + 1) + " / " + slides.Length;
    }
}