using UnityEngine;
using UnityEngine.Events;

public class DoorClick : MonoBehaviour
{
    public UnityEvent onClick;
    void OnMouseDown() { onClick.Invoke(); }
}