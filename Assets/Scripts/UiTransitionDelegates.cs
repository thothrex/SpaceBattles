using System;

namespace SpaceBattles
{
    public delegate void
            UiTransitionRequestHandler
                (UiElementTransition requestedTransition);
    public delegate void UiTransitionBacktrackHandler();
}

