using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBattles
{
    public class SliderAndTextBoxInput : MonoBehaviour
    {
        // -- Fields --
        public InputField TextInput;
        public VariableTextboxPrinter TextboxPrinter;
        public Slider SliderInput;
        public SliderUpdater SliderUpdater;
        // The following can be overriden in Unity editor
        [Tooltip("Currently unused")]
        public double MaxValue = double.MaxValue;
        [Tooltip("Currently unused")]
        public double MinValue = double.MinValue;

        private static readonly double EquatableDoubleDifference = 0.0001;
        private double StoredValue = 0.0;
        private OptionalEventModule OEM = new OptionalEventModule();
        // relied on for propagating events
        private bool Initialised = false;
        private System.Object InitialisedLock = new System.Object();

        // -- Delegates --
        public delegate void NumericalSettingHandler(double settingValue);

        // -- Events --
        public event NumericalSettingHandler FrameRateCapSet;

        public void Awake()
        {
            MyContract.RequireFieldNotNull(
                TextInput,
                "Text Input Component"
            );
            MyContract.RequireFieldNotNull(
                TextboxPrinter,
                "Variable Textbox Printer Component"
            );
            MyContract.RequireFieldNotNull(
                SliderInput,
                "Slider Input Component"
            );
            MyContract.RequireFieldNotNull(
                SliderUpdater,
                "Slider Updater Component"
            );
        }

        public void DisplayValue (double desiredValue)
        {
            lock (InitialisedLock)
            {
                // If already initialised and trying to set the value to
                // the same one as is already stored,
                // this routine can return immediately.
                // This scenario may occur when the UIManager swaps back to
                // the settings menu after the first time.
                bool DesiredValueIsDifferentToStoredValue
                    = Math.Abs(desiredValue - StoredValue)
                    < EquatableDoubleDifference;
                if (!(Initialised && DesiredValueIsDifferentToStoredValue))
                {
                    if (Initialised)
                    {
                        throw new InvalidOperationException(
                            GenerateAlreadyInitialisedExceptionMessage(desiredValue)
                        );
                    }
                    StoredValue = desiredValue;
                    // propagate to components
                    SliderUpdater.UpdateValue(Convert.ToSingle(desiredValue));
                    TextboxPrinter.PrintVariable(desiredValue);
                    Initialised = true;
                }
            }
        }

        public void SetValue(Single newValue)
        {
            SetValue((double)newValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="OverflowException"></exception>
        /// <param name="newValue"></param>
        public void SetValue(string newValue)
        {
            SetValue(Convert.ToDouble(newValue));
        }

        public void SetValue(double newValue)
        {
            MyContract.RequireFieldNotNull(
                TextInput,
                "Text Input Component"
            );
            MyContract.RequireFieldNotNull(
                SliderInput,
                "SliderInput Component"
            );
            StoredValue = newValue;

            var e = FrameRateCapSet;
            lock (InitialisedLock)
            {
                if (Initialised // short-circuit required
                &&  OEM.shouldTriggerEvent(e))
                {
                    e.Invoke(newValue);
                }
            }
            // Updates are propagated between the two input components
            // in the editor
            // (via 3rd-party components - SliderUpdate & VariableTextBoxPrinter)
        }

        private string
        GenerateAlreadyInitialisedExceptionMessage
            (double desiredValue)
        {
            return "This SliderAndTextBoxInput has already been initialised.\n"
                 + "Current stored value: " + StoredValue
                 + "\tDesired value: " + desiredValue;
        }
    }
}


