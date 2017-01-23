using System;
using UnityEngine;

namespace SpaceBattles
{
    public class OrreryUIManager : MonoBehaviour
    {
        // -- Fields --
        public DateTimePicker DateTimePicker;
        public ScalePicker ScalePicker;
        private OptionalEventModule oem = new OptionalEventModule();

        // -- Delegates --
        public delegate void ExplicitDateTimeSetHandler (DateTime newTime);
        public delegate void ScaleSetHandler (float scaledValue, Scale scale);

        // -- Events --
        public event ExplicitDateTimeSetHandler DateTimeSet;
        public event ScaleSetHandler PlanetScaleSet;

        public void BroadcastNewDateTime ()
        {
            MyContract.RequireFieldNotNull(DateTimePicker, "DateTimePicker");
            if (oem.shouldTriggerEvent(DateTimeSet))
            {
                DateTimeSet(DateTimePicker.CurrentStoredValue);
            }
        }

        public void BroadcastNewScale ()
        {
            Debug.Log(
                "Broadcasting new scale: "
                + ScalePicker.CurrentScaledValue
                + ", "
                + ScalePicker.CurrentScale
            );
            MyContract.RequireFieldNotNull(ScalePicker, "ScalePicker");
            if (oem.shouldTriggerEvent(PlanetScaleSet))
            {
                PlanetScaleSet(
                    ScalePicker.CurrentScaledValue,
                    ScalePicker.CurrentScale
                );
            }
        }
    }
}