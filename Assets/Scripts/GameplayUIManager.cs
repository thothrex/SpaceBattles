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
        
        private GameObjectRegistryModule ComponentRegistry
            = new GameObjectRegistryModule();

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
            GameObject Joystick
                = ComponentRegistry
                .RetrieveGameObject((int)UIElements.VirtualJoystick);
            GameObject AccelerateButton
                = ComponentRegistry
                .RetrieveGameObject((int)UIElements.AccelerateButton);

            MyContract.RequireFieldNotNull(Joystick,
                                          "Joystick Component");
            MyContract.RequireFieldNotNull(AccelerateButton,
                                           "Accelerate Button Component");

            Joystick.SetActive(joystickActive);
            Joystick.GetComponent<Image>().raycastTarget
                = joystickActive;
            AccelerateButton.SetActive(!joystickActive);
            AccelerateButton.GetComponentInChildren<Image>().raycastTarget
                = !joystickActive;
        }
    }
}

