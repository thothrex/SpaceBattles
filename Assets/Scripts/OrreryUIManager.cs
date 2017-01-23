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
        public delegate void LinearScaleSetHandler (float scale);
        public delegate void LogScaleSetHandler (float logBase, float innerMultiplier, float outerMultiplier);

        // -- Events --
        public event ExplicitDateTimeSetHandler DateTimeSet;
        public event LinearScaleSetHandler PlanetLinearScaleSet;
        public event LogScaleSetHandler PlanetLogarithmicScaleSet;

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
            MyContract.RequireFieldNotNull(ScalePicker, "ScalePicker");
            Debug.Log(
                "Broadcasting new "
                + ScalePicker.CurrentScaleType
                + " scale"
            );

            ScalePicker.ScaleOption ScaleOption = ScalePicker.CurrentScaleType;
            switch (ScaleOption)
            {
                case ScalePicker.ScaleOption.Linear:
                    if (oem.shouldTriggerEvent(PlanetLinearScaleSet))
                    {
                        PlanetLinearScaleSet(
                            ScalePicker.CurrentLinearScale
                        );
                    }
                    break;
                case ScalePicker.ScaleOption.Logarithmic:
                    if (oem.shouldTriggerEvent(PlanetLogarithmicScaleSet))
                    {
                        PlanetLogarithmicScaleSet(
                            ScalePicker.CurrentLogBase,
                            ScalePicker.CurrentLogInnerMultiplier,
                            ScalePicker.CurrentLogOuterMultiplier
                        );
                    }
                    break;
                default:
                    throw new UnexpectedEnumValueException
                        <ScalePicker.ScaleOption>(ScaleOption);
            }
            
            
        }
    }
}