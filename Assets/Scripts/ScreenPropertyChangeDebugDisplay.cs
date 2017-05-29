using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBattles
{
    public class ScreenPropertyChangeDebugDisplay : MonoBehaviour
    {
        public Text ProvidedTextbox;
        public Canvas FixedScreenCanvas;

        private const string NO_TEXTBOX_EXC
            = "VariableTextboxPrinter is not attached to a Unity GameObject "
            + "with a GUIText element.";

        private Text Textbox;
        // NB: The input field requires you to use a valid stirng
        private InputField InputTextbox;
        private Resolution PreviousResolution;
        private ScreenOrientation PreviousOrientation;
        private DateTime LastScreenChange;
        private DateTime LastPrint;

        public void Awake()
        {
            if (ProvidedTextbox != null)
            {
                Textbox = ProvidedTextbox;
            }
            else
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
        }

        public void Update ()
        {
            Resolution CurrentResolution = Screen.currentResolution;
            ScreenOrientation CurrentOrientation = Screen.orientation;
            bool ResolutionChanged
                = CurrentResolution.height != PreviousResolution.height
                || CurrentResolution.width != PreviousResolution.width;
            bool OrientationChanged = CurrentOrientation != PreviousOrientation;

            if (ResolutionChanged || OrientationChanged)
            {
                LastScreenChange = DateTime.Now;
            }
            if ((DateTime.Now - LastPrint).TotalSeconds > 1.0)
            {
                LastPrint = DateTime.Now;
                PrintVariable(
                    CreateOutputString(ResolutionChanged, OrientationChanged)
                );
            }
            PreviousOrientation = CurrentOrientation;
            PreviousResolution = CurrentResolution;
        }

        public void PrintVariable(String variable)
        {
            MyContract.RequireArgumentNotNull(variable, "variable");
            MyContract.RequireFieldNotNull(Textbox, "textbox");
            Textbox.text = variable.ToString();
        }

        private string
        CreateOutputString 
           (bool ResolutionChanged, bool OrientationChanged)
        {
            string ReturnString
                = "Time since last screen change: "
                + (DateTime.Now - LastScreenChange).TotalSeconds;
            if (!(ResolutionChanged && OrientationChanged))
            {
                ReturnString
                    += "\nOnly the "
                    + (ResolutionChanged ? "resolution" : "orientation")
                    + " changed.";
            }
            ReturnString
                += "Current resolution: "
                + Screen.currentResolution.ToString()
                + "\nCurrent orientation: "
                + Screen.orientation.ToString();
            if (FixedScreenCanvas != null)
            {
                ReturnString
                    += "\nFixedScreenCanvas pixel rect: "
                    + FixedScreenCanvas.pixelRect;
            }
            return ReturnString;
        }
    }
}

