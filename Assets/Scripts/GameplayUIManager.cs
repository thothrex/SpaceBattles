using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBattles
{
    public class GameplayUIManager : MonoBehaviour
    {
        // -- Fields --
        public UIBarManager LocalPlayerHealthBar;
        public List<GameObject> SubComponents;

        private static readonly string ActivateUIElementConditionDescription
            = "is equal to Accelerate button, fire button or virtual joystick";
        private UIRegistry ComponentRegistry = new UIRegistry();
        private UIElements ActiveElements = UIElements.None;

        // -- Methods --

        public void
        InitialiseSubComponents
            (IScreenSizeBreakpointRegister register)
        {
            ComponentRegistry.RegisterUiGameObjects(SubComponents, register);
        }

        public void LocalPlayerSetMaxHealth (double value)
        {
            LocalPlayerHealthBar.SetMaxValue(value);
        }

        public void LocalPlayerSetCurrentHealth (double value)
        {
            LocalPlayerHealthBar.SetCurrentValue(value);
        }

        public void ActivateVirtualJoystick (bool joystickActive)
        {
            //Debug.Log(
            //    "Setting virtual joystick "
            //    + (joystickActive ? "active" : "inactive")
            //);
            ActivateUIElement(UIElements.VirtualJoystick, joystickActive);
        }

        public void ActivateAccelerateButton (bool buttonActive)
        {
            //Debug.Log(
            //    "Setting accelerate button "
            //    + (buttonActive ? "active" : "inactive")
            //);
            ActivateUIElement(UIElements.AccelerateButton, buttonActive);
        }

        public void ActivateFireButton(bool buttonActive)
        {
            Debug.Log(
                "Setting fire button "
                + (buttonActive ? "active" : "inactive")
            );
            ActivateUIElement(UIElements.FireButton, buttonActive);
            // TODO: Make the click interceptor its own thing
            // Need to make 'choose one' UI elements first though
            // otherwise we still run into the issue where
            // these two can both be active together,
            // which is not what we want.
            ActivateUIElement(UIElements.ClickInterceptor, !buttonActive);
        }

        private void ActivateUIElement (UIElements elementsToActivate, bool active)
        {
            MyContract.RequireArgument(
                   elementsToActivate == UIElements.AccelerateButton
                || elementsToActivate == UIElements.VirtualJoystick
                || elementsToActivate == UIElements.FireButton
                || elementsToActivate == UIElements.ClickInterceptor
                ,
                   ActivateUIElementConditionDescription,
                   "elementsToActivate"
            );
            GameObject go
                = ComponentRegistry[(int)elementsToActivate];
            MyContract.RequireFieldNotNull(
                go, elementsToActivate.ToString()
            );
            
            go.SetActive(active);
            foreach (Image i in go.GetComponentsInChildren<Image>())
            {
                i.raycastTarget = active;
            }
        }
    }
}

