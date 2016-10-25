using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBattles
{
    public class VariableTextboxPrinter : MonoBehaviour
    {
        private const string NO_TEXTBOX_EXC
            = "VariableTextboxPrinter is not attached to a Unity GameObject "
            + "with a GUIText element.";

        private Text textbox;

        public void Start ()
        {
            textbox = GetComponent<Text>();
            if (textbox == null)
            {
                throw new InvalidOperationException(NO_TEXTBOX_EXC);
            }
        }

        public void PrintVariable (Rect variable)
        {
            MyContract.RequireArgumentNotNull(variable, "variable");
            MyContract.RequireFieldNotNull(textbox, "textbox");
            textbox.text = variable.ToString();
        }

        public void printVariable<T>(T variable)
        {
            textbox.text = variable.ToString();
        }
    }
}

