using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceBattles
{
    public class ScreenSizeChangeLogic : IScreenSizeRegister
    {
        private const string IMPROPER_LIST_ORDER_WARNING
            = "A list of screen size change triggers has been passed "
            + "to this class in an incorrect ordering. "
            + "Please ensure that clients of this class pass in their "
            + "triggers already sorted in the correct order "
            + "(using FloatInverseOrderComparer)";

        private const string ADD_BREAKPOINT_EXISTING_OBJECT_WARN
            = "Adding breakpoint handlers to existing breakpoints "
            + "for this object. Breakpoints should only be initialised "
            + "for an object once.";

        private enum Dimension { WIDTH, HEIGHT };

        private SortedList<float, SortedList<float, ScreenBreakpointHandler>>
            screen_width_breakpoint_triggers;
        private SortedList<float, SortedList<float, ScreenBreakpointHandler>>
            screen_height_breakpoint_triggers;
        private Dictionary<object, SortedList<float, ScreenBreakpointHandler>>
            width_breakpoint_registrants;
        private Dictionary<object, SortedList<float, ScreenBreakpointHandler>>
            height_breakpoint_registrants;

        public delegate void ScreenBreakpointHandler();



        public ScreenSizeChangeLogic ()
        {
            screen_width_breakpoint_triggers
                = new SortedList<float, SortedList<float, ScreenBreakpointHandler>>(
                    new FloatInverseOrderAllowDuplicatesComparer());

            screen_height_breakpoint_triggers
                = new SortedList<float, SortedList<float, ScreenBreakpointHandler>>(
                    new FloatInverseOrderAllowDuplicatesComparer());

            width_breakpoint_registrants
                = new Dictionary<object, SortedList<float, ScreenBreakpointHandler>>();
            height_breakpoint_registrants
                = new Dictionary<object, SortedList<float, ScreenBreakpointHandler>>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="object_breakpoints">
        /// This needs to be sorted using the FloatInverseOrderComparer
        /// </param>
        /// <param name="registrant">
        /// The object registering these breakpoints.
        /// Needed to ensure deduplication of triggers.
        /// </param>
        public void registerWidthBreakpointHandlers
            (SortedList<float, ScreenBreakpointHandler> object_breakpoints,
             object registrant)
        {
            registerBreakpoints(
                object_breakpoints, registrant, Dimension.WIDTH
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="object_breakpoints">
        /// This needs to be sorted using the FloatInverseOrderComparer
        /// </param>
        /// <param name="registrant">
        /// The object registering these breakpoints.
        /// Needed to ensure deduplication of triggers.
        /// </param>
        public void registerHeightBreakpointHandlers
            (SortedList<float, ScreenBreakpointHandler> object_breakpoints,
             object registrant)
        {
            registerBreakpoints(
                object_breakpoints, registrant, Dimension.HEIGHT
            );
        }

        /// <summary>
        /// Prerequisite: the lists are sorted in inverse order
        /// i.e. largest trigger value to smallest
        /// </summary>
        public void screenSizeChangeHandler (Rect new_size)
        {
            /// Want to trigger breakpoints when the screen size changes to 
            /// a size smaller than that breakpoint
            /// i.e. we trigger all breakpoints larger than the new dimension.
            /// However, we also only want to trigger the smallest breakpoint
            /// per subscribed object (so as to avoid multiple breakpoint 
            /// calls on each object).
            
            //Debug.Log("Width triggers: ");
            //Debug.Log(printTriggers(screen_width_breakpoint_triggers));
            //Debug.Log("Height triggers: ");
            //Debug.Log(printTriggers(screen_height_breakpoint_triggers));

            triggerLargestBreakpointBelow(Dimension.HEIGHT, new_size.height);
            triggerLargestBreakpointBelow(Dimension.WIDTH, new_size.width);
        }

        private void registerBreakpoints
            (SortedList<float, ScreenBreakpointHandler> object_breakpoints,
             object registrant,
             Dimension dimension)
        {
            Dictionary<object, SortedList<float, ScreenBreakpointHandler>>
                object_registry = null;
            SortedList<float, SortedList<float, ScreenBreakpointHandler>>
                trigger_list = null;

            switch (dimension)
            {
                case Dimension.HEIGHT:
                    object_registry = height_breakpoint_registrants;
                    trigger_list = screen_height_breakpoint_triggers;
                    break;
                case Dimension.WIDTH:
                    object_registry = width_breakpoint_registrants;
                    trigger_list = screen_width_breakpoint_triggers;
                    break;
                default:
                    throw new UnexpectedEnumValueException<Dimension>(dimension);
            }

            registerBreakpoints(object_breakpoints,
                                registrant,
                                object_registry,
                                trigger_list);
        }

        private void registerBreakpoints
            (SortedList<float, ScreenBreakpointHandler> object_breakpoints,
             object registrant,
             Dictionary<object, SortedList<float, ScreenBreakpointHandler>> object_registry,
             SortedList<float, SortedList<float, ScreenBreakpointHandler>> trigger_list)
        {
            var sorted_object_breakpoints
                = ensureTriggersAreInCorrectOrder(object_breakpoints);

            if (object_registry.ContainsKey(registrant)
            && object_registry[registrant] != null)
            {
                // Deduplication
                Debug.LogWarning(ADD_BREAKPOINT_EXISTING_OBJECT_WARN);
                var existing_breakpoints = object_registry[registrant];
                var new_breakpoints
                    = new SortedList<float, ScreenBreakpointHandler>(
                        new FloatInverseOrderAllowDuplicatesComparer()
                      );
                foreach (var breakpoint in object_breakpoints.Concat(existing_breakpoints))
                {
                    new_breakpoints.Add(breakpoint.Key, breakpoint.Value);
                }
                var new_key = new_breakpoints.Keys.Max();
                var existing_index = trigger_list.IndexOfValue(existing_breakpoints);
                // TODO: lock? We want this to be atomic
                object_registry.Remove(registrant);
                trigger_list.RemoveAt(existing_index);

                object_registry.Add(registrant, new_breakpoints);
                trigger_list.Add(new_key, new_breakpoints);
                Debug.Log("Pulled old breakpoints: "
                         + printBreakpoints(existing_breakpoints));
                Debug.Log("Added new breakpoints: "
                         + printBreakpoints(object_breakpoints));
                Debug.Log("Created concatenated breakpoints: "
                         + printBreakpoints(new_breakpoints));
            }
            else
            {
                float largest_breakpoint = sorted_object_breakpoints.Keys.Max();
                object_registry.Add(registrant, object_breakpoints);
                trigger_list.Add(largest_breakpoint, object_breakpoints);
                Debug.Log("Added breakpoints normally");
            }
        }

        /// <summary>
        /// Triggers the largest breakpoint in the input SortedList of input
        /// breakpoints, such that that breakpoint is larger than the given
        /// input value (either height or width)
        /// </summary>
        private void triggerLargestBreakpointBelow
            (SortedList<float, SortedList<float, ScreenBreakpointHandler>> breakpoints,
             float size)
        {
            var trigger_enumerator
                = breakpoints.GetEnumerator();
            while (trigger_enumerator.MoveNext()
              && trigger_enumerator.Current.Key > size)
            {
                float object_largest_triggered_breakpoint
                    = trigger_enumerator.Current.Key;
                ScreenBreakpointHandler object_largest_triggered_function
                    = null;
                var per_object_trigger_enumerator
                    = trigger_enumerator.Current.Value.GetEnumerator();
                while (per_object_trigger_enumerator.MoveNext()
                  && per_object_trigger_enumerator.Current.Key > size)
                {
                    object_largest_triggered_breakpoint
                        = per_object_trigger_enumerator.Current.Key;
                    object_largest_triggered_function
                        = per_object_trigger_enumerator.Current.Value;
                }
                // trigger this object's largest breakpoint
                var current_object_breakpoints
                    = trigger_enumerator.Current.Value;
                Debug.Log("Invoking breakpoint at value "
                         + object_largest_triggered_breakpoint);
                Debug.Log(printTriggers(breakpoints));
                if (object_largest_triggered_function != null)
                {
                    object_largest_triggered_function.Invoke();
                }
                else
                {
                    Debug.Log("WTF no function?");
                }
            }
        }

        private void triggerLargestBreakpointBelow(Dimension dimension, float size)
        {
            SortedList<float, SortedList<float, ScreenBreakpointHandler>>
                breakpoints = null;
            switch (dimension)
            {
                case Dimension.HEIGHT:
                    breakpoints = screen_height_breakpoint_triggers;
                    break;
                case Dimension.WIDTH:
                    breakpoints = screen_width_breakpoint_triggers;
                    break;
                default:
                    throw new UnexpectedEnumValueException<Dimension>(dimension);
            }
            triggerLargestBreakpointBelow(breakpoints, size);
        }

        /// <summary>
        /// Checks if the triggers are in the expected order (largest to smallest)
        /// if they are, it simply returns the input list,
        /// if they are not, it creates a new list using the proper comparator
        ///    and populates this new list with the old elements
        ///    returning this new list.
        /// </summary>
        /// <param name="input_list"></param>
        /// <returns>
        /// A list of the input elements sorted using this class's expected comparator
        /// </returns>
        private SortedList<float, ScreenBreakpointHandler>
        ensureTriggersAreInCorrectOrder(SortedList<float, ScreenBreakpointHandler> input_list)
        {
            // Even though we ask clients to sort correctly,
            // we will still handle the case where they mess it up
            if (!isSortedInInverseOrder(input_list))
            {
                Debug.LogWarning(IMPROPER_LIST_ORDER_WARNING);
                // rebuild the list
                var sorted_object_breakpoints
                    = new SortedList<float, ScreenBreakpointHandler>(
                        new FloatInverseOrderAllowDuplicatesComparer()
                      );
                foreach (var entry in input_list)
                {
                    sorted_object_breakpoints.Add(entry.Key, entry.Value);
                }
                return sorted_object_breakpoints;
            }
            else
            {
                return input_list;
            }
        }

        private bool isSortedInInverseOrder<T>(SortedList<float, T> list)
        {
            var enumerator
                = list.GetEnumerator();
            // need to do this to make current non-null
            if (!enumerator.MoveNext())
            {
                return true; // trivially for no elements this is true
            }
            float prev_key = enumerator.Current.Key;
            while (enumerator.MoveNext() && enumerator.Current.Key <= prev_key)
            {
                prev_key = enumerator.Current.Key;
            }

            // if we reached the end of the list successfully, return true
            // else return false
            return (!enumerator.MoveNext());
        }

        private string printTriggers
            (SortedList<float, SortedList<float, ScreenBreakpointHandler>> trigger_list)
        {
            string returnstring = "";
            foreach (var obj_triggers in trigger_list)
            {
                returnstring += obj_triggers.Key.ToString();
                returnstring += printBreakpoints(obj_triggers.Value);
            }
            return returnstring;
        }

        private string printBreakpoints (SortedList<float, ScreenBreakpointHandler> breakpoints)
        {
            string returnstring = " (";
            foreach (var breakpoint_trigger in breakpoints)
            {
                returnstring += "[";
                returnstring += breakpoint_trigger.Key.ToString();
                returnstring += "] ";
            }
            returnstring += ")\n";
            return returnstring;
        }
    }
}