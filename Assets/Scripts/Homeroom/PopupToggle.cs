using UnityEngine;

public class PopupToggle : MonoBehaviour
{
    public GameObject popup;
    public Collider2D[] blockWhileOpen;   // laptop, window, door, task note...

    public void Open()
    {
        popup.SetActive(true);
        SetColliders(false);
    }

    public void Close()
    {
        popup.SetActive(false);
        SetColliders(true);
    }

    void SetColliders(bool on)
    {
        foreach (var c in blockWhileOpen)
            if (c != null) c.enabled = on;
    }
}