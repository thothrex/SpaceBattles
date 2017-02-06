using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBattles
{
    public class SliderUpdater : MonoBehaviour
    {
        private Slider Slider;

        public void Start()
        {
            Slider = GetComponent<Slider>();
            MyContract.RequireFieldNotNull(Slider, "Attached Slider component");
        }

        public void UpdateValue(String newValue)
        {
            try
            {
                float InterpretedValue = Convert.ToSingle(newValue);
                UpdateValue(InterpretedValue);
            }
            catch (FormatException fe)
            {
                // Just ignore this, it's okay
                // propagate other exceptions
            }
        }

        public void UpdateValue (float newValue)
        {
            MyContract.RequireFieldNotNull(Slider, "Slider");
            Slider.value = newValue;
        }
    }
}

