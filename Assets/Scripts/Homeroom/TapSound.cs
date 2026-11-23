using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class TapSound : MonoBehaviour
{
    public static TapSound Instance;

    public AudioClip tapClip;
    public AudioMixerGroup sfxGroup;                          // so the SFX slider controls it
    [Range(0f, 1f)] public float volume = 0.8f;
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);    // tiny random change so it doesn't sound robotic
    public string[] mutedScenes;                              // scenes that have their own tap sounds

    AudioSource src;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.outputAudioMixerGroup = sfxGroup;
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;   // a screen touch counts as a mouse press

        string scene = SceneManager.GetActiveScene().name;
        foreach (string s in mutedScenes)
            if (s == scene) return;

        Play();
    }

    public void Play()
    {
        if (tapClip == null) return;
        src.pitch = Random.Range(pitchRange.x, pitchRange.y);
        src.PlayOneShot(tapClip, volume);
    }
}