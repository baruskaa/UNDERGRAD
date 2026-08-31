using UnityEngine;
using UnityEngine.UI;

public class Hungerbar : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Slider slider;
    public Gradient sliderGradientHunger;
    public Image fill;
    public void SetMaxHunger(int maxHunger)
    {
        slider.maxValue = maxHunger;
        slider.value = maxHunger;
        fill.color = sliderGradientHunger.Evaluate(1f);

    }
    public void SetHunger(int hunger)
    {
        slider.value = hunger;
        fill.color = sliderGradientHunger.Evaluate(slider.normalizedValue);
    }
    
}
