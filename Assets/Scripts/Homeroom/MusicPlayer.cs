using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;

    [System.Serializable]
    public class SceneTrack { public string sceneName; public AudioClip clip; }

    public AudioMixer mixer;
    public AudioClip defaultTrack;        // plays everywhere unless a scene below overrides it
    public SceneTrack[] sceneTracks;      // optional: a different song for a particular scene

    AudioSource src;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        src = GetComponent<AudioSource>();
        src.loop = true;
        src.playOnAwake = false;

        ApplyVolume("MusicVol", PlayerPrefs.GetFloat("MusicVolume", 0.7f));
        ApplyVolume("SfxVol", PlayerPrefs.GetFloat("SfxVolume", 0.6f));

        SceneManager.sceneLoaded += OnSceneLoaded;
        Play(Pick(SceneManager.GetActiveScene().name));
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) { Play(Pick(scene.name)); }

    AudioClip Pick(string sceneName)
    {
        foreach (var t in sceneTracks)
            if (t.sceneName == sceneName && t.clip != null) return t.clip;
        return defaultTrack;
    }

    void Play(AudioClip clip)
    {
        if (clip == null) return;
        if (src.clip == clip && src.isPlaying) return;   // same song: let it keep going
        src.clip = clip;
        src.Play();
    }

    void ApplyVolume(string param, float value)
    {
        if (mixer != null)
            mixer.SetFloat(param, Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
    }
}