using System;
using System.Reflection;
using UnityEngine;
using BreakpointHandler = SpaceBattles.ScreenSizeChangeLogic.ScreenBreakpointHandler;

namespace SpaceBattles
{
    /// <summary>
    /// One breakpoint as entered into an object's list of breakpoints
    /// </summary>
    [Serializable]
    public class BreakpointEntry
    {
        public Dimension dimension;
        public float breakpoint;
        public MonoBehaviour callback_instance;
        public BreakpointHandler handler;
        /// <summary>
        /// Optional field, but necessary for the editor view
        /// </summary>
        public string handler_function_name;

        public BreakpointEntry()
        {
            dimension = Dimension.WIDTH;
            breakpoint = 0.0f;
            handler = null;
            handler_function_name = "";
        }

        override
        public string ToString()
        {
            return
                  "[Dimension: " + dimension.ToString()
                + ", Breakpoint: " + breakpoint.ToString()
                + ", Handler is " + (handler == null ? "not set" : "set")
                + " to "+ handler_function_name + "]";
        }

        public void rebuildHandlerFromFunctionName ()
        {
            MyContract.RequireFieldNotNull(callback_instance,
                                          "callback_instance");
            MyContract.RequireFieldNotNull(handler_function_name,
                                          "handler_function_name");
            Type callback_instance_type = callback_instance.GetType();
            MethodInfo callback_method
                = callback_instance_type.GetMethod(handler_function_name);
            this.handler = delegate ()
            {
                MyContract.RequireFieldNotNull(
                    callback_method, "callback_method"
                );
                MyContract.RequireArgumentNotNull(
                    callback_instance, "callback_instance"
                );
                //Debug.Log("Invoking delegate function "
                //         + handler_function_name
                //         + " | "
                //         + callback_method.Name);
                try
                {
                    callback_method.Invoke(callback_instance, null);
                }
                catch (Exception e)
                {
                    Debug.Log("An exception happened when invoking a callback: "
                            + e.Message);
                }
            };
        }
    }
}
