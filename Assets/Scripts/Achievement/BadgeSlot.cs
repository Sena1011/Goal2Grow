using UnityEngine;
using UnityEngine.UI;

public class BadgeSlot : MonoBehaviour
{
    [SerializeField] string flowerId;
    [SerializeField] Image badgeImage;
    [SerializeField] bool isMiniGame;
    [SerializeField] Color lockedColor = new Color(0.25f, 0.25f, 0.25f, 1f);

    void Awake() { LoadSprite(); }

    public void Init(string id, bool miniGame, Image image)
    {
        flowerId = id;
        isMiniGame = miniGame;
        badgeImage = image;
        LoadSprite();
        Refresh();
    }

    void LoadSprite()
    {
        if (badgeImage != null && badgeImage.sprite == null)
            badgeImage.sprite = Resources.Load<Sprite>("Badges/" + flowerId);
    }

    void OnEnable()
    {
        var dm = PlayerDataManager.Instance;
        if (dm != null) dm.OnChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        var dm = PlayerDataManager.Instance;
        if (dm != null) dm.OnChanged -= Refresh;
    }

    void Refresh()
    {
        if (badgeImage == null) return;
        var dm = PlayerDataManager.Instance;
        bool earned = dm != null && (isMiniGame ? dm.HasMiniGameBadge(flowerId) : dm.HasBloomed(flowerId));
        badgeImage.color = earned ? Color.white : lockedColor;
    }
}