using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodeHuntGame : MonoBehaviour
{
    int[] secretCode = new int[3];
    int[] currentGuess = new int[3];

    int currentPosition = 0;
    int currentAttempt = 0;

  
    TMP_Text[] codeTexts;
    Image[] guessBoxes;
    TMP_Text[] guessTexts;
    Button[] guessButtons;

    public Button playAgainButton;
    public MinigameEnd minigameEnd; 

    public Color normalColor = new Color(1f, 0.945f, 0.882f);
    public Color greenColor = Color.green;
    public Color orangeColor = new Color(1f, 0.5f, 0f);
    public Color redColor = Color.red;

    Transform[] sceneObjects;
    bool missingReference = false;

    void Awake()
    {
        AutoWire();
    }

    void Start()
    {
        if (missingReference)
        {
            Debug.LogError("CodeHuntGame: fix the missing objects listed above, then press Play again.");
            return;
        }

        StartNewRound();
    }

    // ---------- Auto wiring ----------

    void AutoWire()
    {
        sceneObjects = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        codeTexts = new TMP_Text[3];
        guessBoxes = new Image[9];
        guessTexts = new TMP_Text[9];
        guessButtons = new Button[3];

        for (int i = 0; i < 3; i++)
        {
            codeTexts[i] = Get<TMP_Text>("CodeBox" + (i + 1));

            guessButtons[i] = Get<Button>("GuessButton" + (i + 1));
            if (guessButtons[i] != null)
                guessButtons[i].onClick.AddListener(SubmitGuess);
        }

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                string boxName = "Guess" + (row + 1) + "Box" + (col + 1);
                guessBoxes[row * 3 + col] = Get<Image>(boxName);
                guessTexts[row * 3 + col] = Get<TMP_Text>(boxName);
            }
        }

        for (int n = 0; n < 10; n++)
        {
            int digit = n;
            Button b = Get<Button>("NumberButton" + n);
            if (b != null)
                b.onClick.AddListener(() => EnterNumber(digit));
        }

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(PlayAgain);
        else
            Debug.LogWarning("CodeHuntGame: Play Again Button is not assigned in the Inspector.");
    }

    T Get<T>(string objectName) where T : Component
{
    string wanted = objectName.ToLower();

    foreach (Transform t in sceneObjects)
    {
        if (t.name.Trim().ToLower() != wanted) continue;

        T found = t.GetComponentInChildren<T>(true);
        if (found != null) return found;
    }

    string report = "Could not find a " + typeof(T).Name + " on or under '" + objectName + "'.";
    foreach (Transform t in sceneObjects)
    {
        if (!t.name.ToLower().Contains(wanted)) continue;

        report += "\n   Found '" + t.name + "' (" + t.name.Length + " characters). Components here and below: ";
        foreach (Component c in t.GetComponentsInChildren<Component>(true))
            report += c.GetType().Name + ", ";
    }

    Debug.LogError(report);
    missingReference = true;
    return null;
}

    // ---------- Game logic ----------

    void StartNewRound()
    {
        GenerateCode();

        currentPosition = 0;
        currentAttempt = 0;

        for (int i = 0; i < codeTexts.Length; i++)
        {
            codeTexts[i].text = "?";
        }

        for (int i = 0; i < guessTexts.Length; i++)
        {
            guessTexts[i].text = "";
        }

        for (int i = 0; i < guessBoxes.Length; i++)
        {
            guessBoxes[i].color = normalColor;
        }

        for (int i = 0; i < currentGuess.Length; i++)
        {
            currentGuess[i] = 0;
        }

        UpdateGuessButtons();

        if (playAgainButton != null)
        {
            playAgainButton.gameObject.SetActive(false);
        }

        if (minigameEnd != null) minigameEnd.ResetForNewRound();
    }

    void GenerateCode()
    {
        secretCode[0] = Random.Range(1, 10);
        secretCode[1] = Random.Range(0, 10);
        secretCode[2] = Random.Range(0, 10);

        Debug.Log("Secret code: " + secretCode[0] + secretCode[1] + secretCode[2]);
    }

    public void EnterNumber(int number)
    {
        if (currentAttempt >= 3)
            return;

        if (currentPosition >= 3)
            return;

        int index = currentAttempt * 3 + currentPosition;

        currentGuess[currentPosition] = number;
        guessTexts[index].text = number.ToString();

        currentPosition++;
    }

    public void SubmitGuess()
    {
        if (currentPosition < 3)
            return;

        CheckGuess();

        if (currentGuess[0] == secretCode[0] &&
            currentGuess[1] == secretCode[1] &&
            currentGuess[2] == secretCode[2])
        {
            RevealCode();
            EndRound();
            if (minigameEnd != null) minigameEnd.Win();
            return;
        }

        currentAttempt++;

        if (currentAttempt >= 3)
        {
            RevealCode();
            EndRound();
            return;
        }

        currentPosition = 0;

        for (int i = 0; i < currentGuess.Length; i++)
        {
            currentGuess[i] = 0;
        }

        UpdateGuessButtons();
    }

    void CheckGuess()
    {
        int startIndex = currentAttempt * 3;

        for (int i = 0; i < 3; i++)
        {
            if (currentGuess[i] == secretCode[i])
            {
                guessBoxes[startIndex + i].color = greenColor;
            }
            else if (NumberExists(currentGuess[i]))
            {
                guessBoxes[startIndex + i].color = orangeColor;
            }
            else
            {
                guessBoxes[startIndex + i].color = redColor;
            }
        }
    }

    bool NumberExists(int number)
    {
        for (int i = 0; i < 3; i++)
        {
            if (secretCode[i] == number)
                return true;
        }

        return false;
    }

    void RevealCode()
    {
        for (int i = 0; i < 3; i++)
        {
            codeTexts[i].text = secretCode[i].ToString();
        }
    }

    void UpdateGuessButtons()
    {
        for (int i = 0; i < guessButtons.Length; i++)
        {
            guessButtons[i].interactable = (i == currentAttempt);
        }
    }

    void EndRound()
    {
        for (int i = 0; i < guessButtons.Length; i++)
        {
            guessButtons[i].interactable = false;
        }

        if (playAgainButton != null)
        {
            playAgainButton.gameObject.SetActive(true);
        }
    }

    public void PlayAgain()
    {
        StartNewRound();
    }
}