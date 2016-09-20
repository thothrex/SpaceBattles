using System;
using UnityEngine;
using UnityEngine.Events;

namespace SpaceBattles
{
    public delegate void PlayGameButtonPressEventHandler();
    public class OptionalEventModule
    {
        private const string NO_EVENT_LISTENERS_ERRMSG
            = "An event which was marked as requiring at least one "
            + "event listener has none. Either add an event listener "
            + "or change the OptionalEventModule's "
            + "allow_no_event_listeners value to false "
            + "(which can be done via the editor or in the parent class)";

        public bool allow_no_event_listeners = false;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="H">The event handler/delegate type</typeparam>
        /// <param name="event_handler"></param>
        /// <returns>
        /// true if their is >= 1 event handler,
        /// false if null and allow_no_event_listeners,
        /// throws an InvalidOperationException if !allow_no_event_listeners
        /// and there are no registered event handlers.
        /// </returns>
        public bool shouldTriggerEvent<H>(H event_handler)
        {
            if (allow_no_event_listeners && event_handler == null)
            {
                Debug.Log("Event " 
                         + event_handler.ToString()
                         + "has no event handler, and is thus superfluous.");
                return false;
            }
            else if (event_handler == null)
            {
                throw new InvalidOperationException(NO_EVENT_LISTENERS_ERRMSG);
            }
            else // event_handler != null
            {
                return true;
            }
        }
    }
}
