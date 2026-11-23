using UnityEngine;

public class PanelToggle : MonoBehaviour
{
    public GameObject panel;

    void Start() { panel.SetActive(false); }

    public void Open() { panel.SetActive(true); }
    public void Close() { panel.SetActive(false); }
}
