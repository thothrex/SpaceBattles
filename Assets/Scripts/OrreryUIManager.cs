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
                    if (oem.shouldTriggerEvent(PlanetLinearScaleSet)
                    &&  ScalePicker.CurrentLinearScale != float.NaN
                    &&  !float.IsInfinity(ScalePicker.CurrentLinearScale))
                    {
                        PlanetLinearScaleSet(
                            ScalePicker.CurrentLinearScale
                        );
                    }
                    break;
                case ScalePicker.ScaleOption.Logarithmic:
                    if (oem.shouldTriggerEvent(PlanetLogarithmicScaleSet)
                    && ScalePicker.CurrentLogBase != float.NaN
                    && !float.IsInfinity(ScalePicker.CurrentLogBase)
                    && ScalePicker.CurrentLogBase != 1
                    && ScalePicker.CurrentLogInnerMultiplier != float.NaN
                    && !float.IsInfinity(ScalePicker.CurrentLogInnerMultiplier)
                    && ScalePicker.CurrentLogOuterMultiplier != float.NaN
                    && !float.IsInfinity(ScalePicker.CurrentLogOuterMultiplier))
                    {
                        //Debug.Log(
                        //    "Passing on log values: "
                        //    + "log base "
                        //    + ScalePicker.CurrentLogBase
                        //    + ", inner multiplier "
                        //    + ScalePicker.CurrentLogInnerMultiplier
                        //    + ", outer multiplier"
                        //    + ScalePicker.CurrentLogOuterMultiplier
                        //);
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