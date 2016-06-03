using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic; // for non-typed List

namespace SpaceBattles
{
    public class ButtonMainMenuPlayGame : MonoBehaviour
    {
        public delegate void PlayGameButtonPressEventHandler();
        public event PlayGameButtonPressEventHandler PlayGameButtonPress;

        public List<GameObject> button_states;
        private int button_state = 0;

        void Start()
        {
            Button b = gameObject.GetComponent<Button>();
            b.onClick.AddListener(buttonPress);
        }

        public void buttonPress()
        {
            if (button_state == 0)
            {
                // trigger public event
                PlayGameButtonPress();
                // internal actions
                advanceButtonState();
            }
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