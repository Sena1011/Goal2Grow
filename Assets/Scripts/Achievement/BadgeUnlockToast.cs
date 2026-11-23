using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class BadgeUnlockToast : MonoBehaviour
{
    public GameObject panel;        // the popup box (starts hidden)
    public Image badgeImage;
    public TMP_Text titleText;
    public float showSeconds = 2.5f;
    public AudioClip unlockSound;
    public AudioMixerGroup sfxGroup;   // SFX slider controls volume
    AudioSource sfx;

    static readonly Dictionary<string, string> Titles = new Dictionary<string, string>
    {
        {"Sunflower","Sunflower Bloom"}, {"Daffodil","Blossoms of Hope"}, {"Tulip","Blooming Elegance"},
        {"Daisy","Daisy's Daydream"}, {"Lavender","Whispers of Purple"}, {"Hyacinth","Violet Symphony"},
        {"Carnation","Petals of Affection"},
        {"FlappyLittleFlyer","Little Flyer"}, {"FlappySkyAdventurer","Sky Adventurer"},
        {"FlappyFeatheredChampion","Feathered Champion"}, {"FlappySkyMaster","Sky Master"}, {"FlappyBirdLegend","Bird Legend"},
        {"MemoryFirstMatch","First Match"}, {"MemoryPerfect","Perfect Memory"},
        {"MemoryDailyThinker","Daily Thinker"}, {"MemoryMindChampion","Mind Champion"},
        {"ConnectFirstConnection","First Connection"}, {"ConnectDotExplorer","Dot Explorer"},
        {"ConnectPatternFinder","Pattern Finder"}, {"ConnectPuzzlePro","Puzzle Pro"},
        {"ConnectDailyConnect","Daily Connect"}, {"ConnectConnectingFlower","Connecting Flower"},
        {"CodeOneShotWonder","One-Shot Wonder"}, {"CodeCracker","Code Cracker"},
        {"CodeSpecialist","Code Specialist"}, {"CodeMasterDecoder","Master Decoder"}, {"CodeUltimateHacker","Ultimate Hacker"},
    };

    readonly Queue<string> queue = new Queue<string>();
    bool running;


    void Awake()
    {
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.outputAudioMixerGroup = sfxGroup;
    }

    void OnEnable()
    {
        if (panel != null) panel.SetActive(false);
        running = false;
        var dm = PlayerDataManager.Instance;
        if (dm == null) return;
        dm.OnFlowerBadgeUnlocked += Enqueue;
        dm.OnMiniGameBadgeUnlocked += Enqueue;
    }

    void OnDisable()
    {
        var dm = PlayerDataManager.Instance;
        if (dm == null) return;
        dm.OnFlowerBadgeUnlocked -= Enqueue;
        dm.OnMiniGameBadgeUnlocked -= Enqueue;
    }

    void Enqueue(string id)
    {
        queue.Enqueue(id);
        if (!running) StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        running = true;
        while (queue.Count > 0)
        {
            string id = queue.Dequeue();
            Sprite s = Resources.Load<Sprite>("Badges/" + id);
            if (s == null) { Debug.LogWarning("No badge picture in Resources/Badges named " + id); continue; }

            badgeImage.sprite = s;
            titleText.text = "Badge unlocked!\n" + (Titles.TryGetValue(id, out string t) ? t : id);
            panel.SetActive(true);
            if (unlockSound != null) sfx.PlayOneShot(unlockSound, 0.9f);
            yield return new WaitForSecondsRealtime(showSeconds);   // works even when a game has paused time
            panel.SetActive(false);
            yield return new WaitForSecondsRealtime(0.25f);
        }
        running = false;
    }
}