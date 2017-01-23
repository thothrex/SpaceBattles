using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    public class DateTimePicker : MonoBehaviour
    {
        // -- Fields --
        public List<GameObject> PickerObjects;

        private bool InputFieldsVisible = true;
        private int CurrentYear = 0;
        private int CurrentMonth = 1;
        private int CurrentDay = 1;
        private int CurrentHour = 0;
        private int CurrentMinute = 0;
        private int CurrentSecond = 0;

        // -- Properties --
        public DateTime CurrentStoredValue
        {
            private set;
            get;
        }

        // -- Methods --

        public void ChangeYear (string input)
        {
            ChangeField(out CurrentYear, input, int.MinValue, int.MaxValue);
        }

        public void ChangeMonth(string input)
        {
            ChangeField(out CurrentMonth, input, 1, 12);
        }

        public void ChangeDay(string input)
        {
            ChangeField(out CurrentDay, input, 1, 31);
        }

        public void ChangeHour(string input)
        {
            ChangeField(out CurrentHour, input, 0, 24);
        }

        public void ChangeMinute(string input)
        {
            ChangeField(out CurrentMinute, input, 0, 60);
        }

        public void ChangeSecond(string input)
        {
            // max - leap seconds
            ChangeField(out CurrentMinute, input, 0, 60);
        }

        public void UpdateStoredValue()
        {
            CurrentStoredValue = new DateTime(
                CurrentYear,
                CurrentMonth,
                CurrentDay,
                CurrentHour,
                CurrentMinute,
                CurrentSecond
            );
        }

        public void ToggleInputFieldsVisible ()
        {
            InputFieldsVisible = !InputFieldsVisible;
            SetInputFieldsVisible(InputFieldsVisible);
        }

        public void HideInputFields ()
        {
            SetInputFieldsVisible(false);
        }

        public void ShowInputFields()
        {
            SetInputFieldsVisible(true);
        }

        public void SetInputFieldsVisible (bool visible)
        {
            foreach (GameObject obj in PickerObjects)
            {
                obj.SetActive(visible);
            }
        }

        private void
        ChangeField
        (out int field, string input, int min, int max)
        {
            int ParsedValue;
            if (int.TryParse(input, out ParsedValue))
            {
                if (ParsedValue > max || ParsedValue < min)
                {
                    Debug.LogWarning("Value out of bounds");
                    field = 1;
                }
                else
                {
                    field = ParsedValue;
                }
            }
            else
            {
                field = 1;
                // suppress editor warnings
                // logic is we leave the field unchanged
                Debug.LogWarning("Non-int string given");
            }
        }
    }
}
