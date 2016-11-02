using System;

namespace SpaceBattles
{
    public class UiElementTransition
    {
        public UiElementTransitionType Type;
        public UIElements Targets;

        public UiElementTransition ()
        {

        }

        public
        UiElementTransition
            (UiElementTransitionType type,
             UIElements targets)
        {
            this.Type = type;
            this.Targets = targets;
        }
    }
}
