using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaSlider : MonoBehaviour
{
    public Slider slider;

    public void SetMaxStamina(float max)
    {
        slider.maxValue = max;
        slider.value = max;
    }

    public void SetStamina(float stamina)
    {
        slider.value = stamina;
    }
}
