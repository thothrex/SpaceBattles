using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic; // for non-typed List

namespace SpaceBattles
{
    public class ButtonMainMenuPlayGame : MonoBehaviour
    {
        public List<GameObject> button_states;
        private int button_state = 0;
        private OptionalEventModule oem = new OptionalEventModule();

        public event PlayGameButtonPressEventHandler PlayGameButtonPress;

        void Start()
        {
            Button b = gameObject.GetComponent<Button>();
            b.onClick.AddListener(buttonPress);
            oem.AllowNoEventListeners = false;
        }

        public void buttonPress()
        {
            if (button_state == 0)
            {
                // trigger public event
                // if there are no listeners we should not trigger the event
                // due to general c# rules, as it causes an exception
                PlayGameButtonPressEventHandler eventTrigger = PlayGameButtonPress;
                if (oem.shouldTriggerEvent(eventTrigger))
                {
                    PlayGameButtonPress();
                }
                // internal actions
                advanceButtonState();
            }
        }

        /// <summary>
        /// Requires the caller to know what each state means
        /// THus should be confined to direct managers and not called by long distance clients
        /// i.e. UIManager calls this, not ProgramInstanceManager
        /// </summary>
        public void setButtonState (int new_state)
        {
            if (new_state > button_states.Count)
            {
                throw new ArgumentOutOfRangeException(
                    "new_state", new_state,
                    "Attempting to set new button state to " + new_state +
                    ", which is greater than the maximum possible state: " + button_states.Count
                );
            }
            else if (new_state < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "new_state", new_state,
                    "Attempting to set new button state to " + new_state +
                    ", which is less than 0 (thus considered invalid)"
                );
            }
            animateButtonStateChange(button_state, new_state);
            button_state = new_state;
        }

        public void advanceButtonState()
        {
            int new_button_state = button_state + 1;
            // loops back to beginning
            if (new_button_state == button_states.Count)
            {
                new_button_state = 0;
            }
            animateButtonStateChange(button_state, new_button_state);
            button_state = new_button_state;
            Debug.Log("Play button advancing to state " + button_state);
        }

        private void animateButtonStateChange(int old_state, int new_state)
        {
            if (old_state >= button_states.Count || old_state < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "old_state", old_state, "button trying to change from an old state "
                                          + "which is out of range of this button's states "
                                          + "(Currently the max is " + button_states.Count + ")"
                );
            }
            if (new_state >= button_states.Count || new_state < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "new_state", new_state, "button trying to change to a new state "
                                          + "which is out of range of this button's states"
                                          + "(Currently the max is " + button_states.Count + ")"
                );
            }
            var old_tweens = button_states[old_state].GetComponentsInChildren<EasyTween>(true);
            foreach (EasyTween tween in old_tweens) {
                //Debug.Log("Openclose being called for old tween");
                tween.OpenCloseObjectAnimation();
            }
            var new_tweens = button_states[new_state].GetComponentsInChildren<EasyTween>(true);
            foreach (EasyTween tween in new_tweens) {
                //Debug.Log("Openclose being called for new tween");
                tween.OpenCloseObjectAnimation();
            }
        }
    }
}