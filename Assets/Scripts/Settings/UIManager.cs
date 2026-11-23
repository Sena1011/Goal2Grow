using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject ProfilePanel;
    [SerializeField] GameObject SettingsPanel;

    void Start() => CloseAll();

    public void OpenProfile()
    {
        CloseAll();
        ProfilePanel.SetActive(true);
    }

    public void OpenSettings()
    {
        CloseAll();
        SettingsPanel.SetActive(true);
    }

    public void CloseAll()
    {
        ProfilePanel.SetActive(false);
        SettingsPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) OpenProfile();
        if (Input.GetKeyDown(KeyCode.S)) OpenSettings();
        if (Input.GetKeyDown(KeyCode.Escape)) CloseAll();
    }
}
