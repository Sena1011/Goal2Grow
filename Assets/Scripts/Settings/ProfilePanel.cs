using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePanel : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] Image AvatarImage;
    [SerializeField] Button AvatarButton;
    [SerializeField] TMP_InputField NameInput;
    [SerializeField] Button EditNameButton;
    [SerializeField] TMP_InputField BioInput;
    [SerializeField] Sprite DefaultAvatar;

    [Header("Stats")]
    [SerializeField] TMP_Text CoinsText, FlowerText, GoalsText;

    PlayerDataManager dm;

    void OnEnable()
    {
        dm = PlayerDataManager.Instance;
        if (dm == null)
        {
            Debug.LogError("ProfilePanel: no PlayerDataManager in the scene.", this);
            return;
        }
        dm.OnChanged += Refresh;
        if (Inventory.Instance != null) Inventory.Instance.OnMoneyChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (dm != null) dm.OnChanged -= Refresh;
        if (Inventory.Instance != null) Inventory.Instance.OnMoneyChanged -= Refresh;
    }

    void Start()
    {
        if (dm == null) return;

        AvatarButton.onClick.AddListener(PickAvatar);
        EditNameButton.onClick.AddListener(() => NameInput.ActivateInputField());

        NameInput.onEndEdit.AddListener(s =>
        {
            if (!string.IsNullOrWhiteSpace(s)) dm.SetName(s.Trim());
            else NameInput.text = dm.Data.Player_Name;
        });
        BioInput.onEndEdit.AddListener(s => dm.SetBio(s.Trim()));
    }

    void Refresh()
    {
        var d = dm.Data;

        if (!NameInput.isFocused) NameInput.text = d.Player_Name;
        if (!BioInput.isFocused)  BioInput.text  = d.Bio;

        CoinsText.text = Inventory.Instance != null
        ? Inventory.Instance.money.ToString()
        : d.Coins.ToString();
        
        FlowerText.text = d.Flowers_Bloomed.ToString();
        GoalsText.text  = d.Goals_Completed.ToString();

        var tex = dm.LoadAvatar();
        if (tex != null) SetAvatar(tex);
        else if (DefaultAvatar != null) AvatarImage.sprite = DefaultAvatar;
    }

    void PickAvatar()
    {
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Choose avatar", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path)) LoadFromPath(path);
#else
        NativeGallery.GetImageFromGallery(path =>
        {
            if (path != null) LoadFromPath(path);
        }, "Select a profile picture");
#endif
    }

    void LoadFromPath(string path)
    {
        var bytes = System.IO.File.ReadAllBytes(path);
        var src = new Texture2D(2, 2);
        src.LoadImage(bytes);

        int size = Mathf.Min(src.width, src.height);
        var square = new Texture2D(size, size);
        square.SetPixels(src.GetPixels((src.width - size) / 2, (src.height - size) / 2, size, size));
        square.Apply();

        dm.SaveAvatar(square);
    }

    void SetAvatar(Texture2D tex)
    {
        AvatarImage.sprite = Sprite.Create(
            tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        AvatarImage.preserveAspect = true;
    }
}