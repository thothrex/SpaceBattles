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

        private Text Textbox;
        // NB: The input field requires you to use a valid stirng
        private InputField InputTextbox;

        public void Start ()
        {
            Textbox = GetComponent<Text>();
            if (Textbox == null)
            {
                InputTextbox = GetComponent<InputField>();
                if (InputTextbox == null)
                {
                    throw new InvalidOperationException(NO_TEXTBOX_EXC);
                }
            }
        }

        public void PrintVariable (Rect variable)
        {
            MyContract.RequireArgumentNotNull(variable, "variable");
            MyContract.RequireFieldNotNull(Textbox, "textbox");
            Textbox.text = variable.ToString();
        }

        public void PrintVariable (float variable)
        {
            PrintVariable<float>(variable);
        }

        // Fallback/default implementation
        public void PrintVariable<T>(T variable)
        {
            if (Textbox != null)
            {
                Textbox.text = variable.ToString();
            }
            else if (InputTextbox != null)
            {
                InputTextbox.text = variable.ToString();
            }
            else
            {
                throw new InvalidOperationException(NO_TEXTBOX_EXC);
            }
        }
    }
}

