using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public void SetToMax(float value)
    {
        SetMaxValue(value);
        SetValue(value);
        fill.color = gradient.Evaluate(1f);
    }

    public void SetMaxValue(float value)
    {
        slider.maxValue = value;
        fill.color = gradient.Evaluate(slider.normalizedValue);

    }

    public void SetValue(float value)
    {
        slider.value = value;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
