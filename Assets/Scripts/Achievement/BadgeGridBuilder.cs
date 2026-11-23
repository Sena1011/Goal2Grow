using UnityEngine;
using UnityEngine.UI;

public class BadgeGridBuilder : MonoBehaviour
{
    public string[] badgeIds;
    public bool isMiniGame = true;

    void Awake()
    {
        foreach (string id in badgeIds)
        {
            var go = new GameObject(id, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);

            var img = go.GetComponent<Image>();
            img.preserveAspect = true;
            img.raycastTarget = false;

            go.AddComponent<BadgeSlot>().Init(id, isMiniGame, img);
        }
    }
}