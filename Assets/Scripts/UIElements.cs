using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    [Serializable]
    [Flags]
    public enum UIElements
    {
        None = 0,
        MainMenu = 1,
        SettingsMenu = 2,
        ShipSelect = 4,
        GameplayUI = 8,
        VirtualJoystick = 16,
        InGameMenu = 32,
        AccelerateButton = 64,
        FireButton = 128,
        DebugOutput = 256,
        // FreeSlot = 512,
        OrreryUI = 1024
    }

    public static class UIElementExtensions
    {
        public static readonly string NoManagerMessage
            = "This UIElement does not have a designated manager class";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiElement"></param>
        /// <returns>
        /// Manager class if it has one, null otherwise
        /// </returns>
        public static Type ManagerClass(this UIElements uiElement)
        {
            switch (uiElement)
            {
                case UIElements.MainMenu:
                    return typeof(MainMenuUIManager);
                case UIElements.SettingsMenu:
                    return typeof(SettingsMenuUIManager);
                case UIElements.GameplayUI:
                    return typeof(GameplayUIManager);
                case UIElements.InGameMenu:
                    return typeof(InGameMenuManager);
                case UIElements.OrreryUI:
                    return typeof(OrreryUIManager);
                default:
                    return null; 
            }
        }
    }
}