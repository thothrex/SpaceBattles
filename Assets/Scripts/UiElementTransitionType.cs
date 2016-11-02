using System;

namespace SpaceBattles
{
    public enum UiElementTransitionType
    {
        /// <summary>
        /// Clears the UI history;
        /// deactivates any active UI elements;
        /// activates the target UI elements.
        /// </summary>
        Fresh,
        /// <summary>
        /// Adds the current state to the UI history;
        /// deactivates any active UI elements;
        /// activates the target UI elements.
        /// </summary>
        Tracked,
        /// <summary>
        /// Activates the target UI elements.
        /// Leaves the UI history unchanged.
        /// </summary>
        Additive,
        /// <summary>
        /// Deactivates the target UI elements.
        /// Leaves the UI history unchanged.
        /// </summary>
        Subtractive
    };
}

