using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] AudioMixer Mixer;
    [SerializeField] Slider MusicSlider;
    [SerializeField] Slider SfxSlider;

    [Header("Buttons")]
    [SerializeField] Button ResetButton;
    [SerializeField] Button SaveCloudButton;

    [Header("Reset confirmation")]
    [SerializeField] GameObject ConfirmPanel;
    [SerializeField] Button ConfirmYesButton;
    [SerializeField] Button ConfirmNoButton;

    [Header("Message (optional)")]
    [SerializeField] TMP_Text MessageText;

    const string MusicKey = "MusicVolume";
    const string SfxKey = "SfxVolume";

    void Start()
    {
        MusicSlider.value = PlayerPrefs.GetFloat(MusicKey, 0.7f);
        SfxSlider.value = PlayerPrefs.GetFloat(SfxKey, 0.6f);
        ApplyVolume("MusicVol", MusicSlider.value);
        ApplyVolume("SfxVol", SfxSlider.value);

        MusicSlider.onValueChanged.AddListener(v => { ApplyVolume("MusicVol", v); PlayerPrefs.SetFloat(MusicKey, v); });
        SfxSlider.onValueChanged.AddListener(v => { ApplyVolume("SfxVol", v); PlayerPrefs.SetFloat(SfxKey, v); });

        ResetButton.onClick.AddListener(OpenConfirm);
        SaveCloudButton.onClick.AddListener(SaveEverything);
        if (ConfirmYesButton != null) ConfirmYesButton.onClick.AddListener(DoReset);
        if (ConfirmNoButton != null) ConfirmNoButton.onClick.AddListener(CloseConfirm);

        if (ConfirmPanel != null) ConfirmPanel.SetActive(false);
        if (MessageText != null) MessageText.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        // always reopen the popup with the confirmation closed
        if (ConfirmPanel != null) ConfirmPanel.SetActive(false);
    }

    void OnDisable() => PlayerPrefs.Save();

    void ApplyVolume(string param, float value)
    {
        float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        Mixer.SetFloat(param, db);
    }

    // ---------- Save ----------

    void SaveEverything()
    {
        if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.Save();
        if (Inventory.Instance != null) Inventory.Instance.Save();
        PlayerPrefs.Save();
        ShowMessage("Progress saved!");
    }

    // ---------- Reset ----------

    void OpenConfirm()
    {
        if (ConfirmPanel != null) ConfirmPanel.SetActive(true);
        else DoReset();   // no confirmation panel set up: reset straight away
    }

    void CloseConfirm()
    {
        if (ConfirmPanel != null) ConfirmPanel.SetActive(false);
    }

    void DoReset()
    {
        if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.ResetProgress();   // profile, badges, stats
        if (Inventory.Instance != null) Inventory.Instance.ResetAll();                        // money, items, plant, garden
        ClearJournal();                                                                      // goals and this week's progress
        CloseConfirm();
        ShowMessage("Progress reset");
    }

    void ClearJournal()
    {
        DateTime today = DateTime.Today;
        int daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
        string weekKey = today.Date.AddDays(-daysSinceMonday).ToString("yyyy-MM-dd");

        PlayerPrefs.DeleteKey("JournalGoals_" + weekKey);
        PlayerPrefs.DeleteKey("JournalDone_" + weekKey);
        PlayerPrefs.DeleteKey("JournalDailyTemplates");   // leftovers from the removed daily goals
        PlayerPrefs.DeleteKey("JournalDailyLastDay");
        PlayerPrefs.DeleteKey("JournalGrowthDay");
        PlayerPrefs.DeleteKey("JournalGrowthToday");
        PlayerPrefs.Save();                               // the volume settings are not touched
    }

    // ---------- Message ----------

    void ShowMessage(string text)
    {
        if (MessageText == null) return;
        MessageText.text = text;
        MessageText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideMessage));
        Invoke(nameof(HideMessage), 2f);
    }

    void HideMessage()
    {
        if (MessageText != null) MessageText.gameObject.SetActive(false);
    }
}