using UnityEngine;
using UnityEngine.UI;

public class GrowthBar : MonoBehaviour
{
    public PlantGrowth plant;
    public Slider slider;

    void Awake()
    {
        if (slider == null) slider = GetComponent<Slider>();
    }

    void Update()
    {
        if (plant != null && slider != null)
            slider.value = plant.Progress;
    }
}