using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public PopupToggle prompt;   // optional: lets the phone's Back button open and close the popup

    public void Confirm()
    {
        // save everything first, in case the app is closed straight afterwards
        if (Inventory.Instance != null) Inventory.Instance.Save();
        if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.Save();
        PlayerPrefs.Save();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   // Application.Quit does nothing in the Editor
#else
        Application.Quit();
#endif
    }

    void Update()
    {
        // Android's Back button counts as Escape
        if (prompt != null && Input.GetKeyDown(KeyCode.Escape))
        {
            if (prompt.popup.activeSelf) prompt.Close(); else prompt.Open();
        }
    }
}