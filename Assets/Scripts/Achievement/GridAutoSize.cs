using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class GridAutoSize : MonoBehaviour
{
    public int columns = 4;

    void OnEnable() { Apply(); }
    void OnRectTransformDimensionsChange() { Apply(); }

    void Apply()
    {
        var g = GetComponent<GridLayoutGroup>();
        var rt = (RectTransform)transform;
        float usable = rt.rect.width - g.padding.left - g.padding.right - g.spacing.x * (columns - 1);
        float size = Mathf.Floor(usable / columns);
        if (size > 0) g.cellSize = new Vector2(size, size);
    }
}