using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Player player;
    [SerializeField] private Spawner spawner;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private int coinsPerPoint = 2;   // coins earned per pipe passed
    [SerializeField] private int maxReward = 100;     // cap so it can't be farmed
    [SerializeField] private Text rewardText;         // optional: shows "+10 coins"

private bool rewarded;

    public int score { get; private set; } = 0;

    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;   // un-freeze when leaving this scene
        if (Instance == this) {
            Instance = null;
        }
    }

    private void Start()
    {
        Pause();
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        player.enabled = false;
    }

    public void Play()
    {
        score = 0;
        scoreText.text = score.ToString();

        rewarded = false;
        if (rewardText != null) rewardText.text = "";

        playButton.SetActive(false);
        gameOver.SetActive(false);

        Time.timeScale = 1f;
        player.enabled = true;

        Pipes[] pipes = FindObjectsOfType<Pipes>();

        for (int i = 0; i < pipes.Length; i++) {
            Destroy(pipes[i].gameObject);
        }
    }

    public void GameOver()
    {
        playButton.SetActive(true);
        gameOver.SetActive(true);

        if (!rewarded)
        {
            rewarded = true;   // prevents paying twice if GameOver is called again
            int reward = Mathf.Min(score * coinsPerPoint, maxReward);

            if (reward > 0 && Inventory.Instance != null)
                Inventory.Instance.AddMoney(reward);

            if (rewardText != null)
                rewardText.text = "+" + reward + " coins";

            if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.ReportFlappyScore(score);
        }

        Pause();
    }

    public void IncreaseScore()
    {
        score++;
        scoreText.text = score.ToString();
    }

}
