using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SpaceBattles
{
    public class UIBarManager : MonoBehaviour
    {
        public GameObject health_bar_background_object;
        public GameObject health_bar_foreground_object;

        public double max_bar_value;
        public double current_bar_value;
        public double scroll_speed = 1.0;

        private RectTransform health_bar_foreground;
        private RectTransform health_bar_background;
        public double bar_max_width;
        public double current_bar_width;
        public double target_bar_width;

        void Awake ()
        {
            health_bar_background = health_bar_background_object.GetComponent<RectTransform>();
            health_bar_foreground = health_bar_foreground_object.GetComponent<RectTransform>();
            bar_max_width = health_bar_background.rect.width;
        }

        const double BAR_WIDTH_EPSILON = 0.1f;

        /// <summary>
        /// Purely for animating the bar
        /// </summary>
        void Update ()
        {
            double difference = target_bar_width - current_bar_width;
            if (Math.Abs(difference) < BAR_WIDTH_EPSILON)
            {
                current_bar_width = target_bar_width;
                this.enabled = false;
            }
            else
            {
                current_bar_width = current_bar_width
                                  + (difference * Time.deltaTime * scroll_speed);
                health_bar_foreground.sizeDelta
                    = new Vector2(Convert.ToSingle(current_bar_width),
                                  health_bar_foreground.rect.height);
            }
        }

        public void SetMaxValue (double max_value)
        {
            this.max_bar_value = max_value;
            this.enabled = true;
        }

        private const string larger_than_possible_error_message
            = "New bar value is larger than this bar's current maximum value";

        private const string smaller_than_possible_error_message
            = "New bar value is less than 0";

        /// <summary>
        /// Simple pass-through overload to the double version
        /// </summary>
        /// <param name="new_value"></param>
        public void setCurrentValue (int new_value)
        {
            SetCurrentValue(Convert.ToDouble(new_value));
        }

        /// <summary>
        /// Simple pass-through overload to the double version
        /// </summary>
        /// <param name="new_value"></param>
        public void setCurrentValue (float new_value)
        {
            SetCurrentValue(Convert.ToDouble(new_value));
        }

        /// <summary>
        /// We assume the caller knows the scale of values used
        /// </summary>
        public void SetCurrentValue (double new_value)
        {
            Debug.Log("Setting new bar value to " + new_value);
            if (new_value > max_bar_value)
            {
                throw new ArgumentOutOfRangeException(
                    "new_value",new_value, larger_than_possible_error_message
                );
            }
            else if (new_value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "new_value", new_value, smaller_than_possible_error_message
                );
            }

            current_bar_value = new_value;
            target_bar_width = (current_bar_value / max_bar_value) * bar_max_width;
            this.enabled = true;
        }
    }
}
