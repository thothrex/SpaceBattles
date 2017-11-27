using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    /// <summary>
    /// Sorry for the horrible hard-coded numbers
    /// </summary>
    public class ScalePicker : MonoBehaviour
    {
        // -- Fields --
        public List<GameObject> LogarithmicScaleOptions;
        public List<GameObject> LinearScaleOptions;
        public ScaleOption InitialScaleOption;
        private ScaleOption CurrentScaleOption;

        // -- Events --

        // -- Enums--
        public enum ScaleOption { Linear, Logarithmic };

        // -- Properties --
        public ScaleOption CurrentScaleType    { private set; get; }
        public float CurrentLinearScale        { private set; get; }
        public float CurrentLogBase            { private set; get; }
        public float CurrentLogInnerMultiplier { private set; get; }
        public float CurrentLogOuterMultiplier { private set; get; }

        // -- Methods --
        public void Awake ()
        {
            //Debug.Log("ScalePicker waking up");
            CurrentScaleOption = InitialScaleOption;
            CurrentLinearScale = 0.0015f;
            CurrentLogBase = 10.0f;
            CurrentLogInnerMultiplier = 1.0f;
            CurrentLogOuterMultiplier = 1.0f;
        }

        public void SetScale (Int32 input)
        {
            Debug.Log("Setting scale choice to " + input);
            MyContract.RequireArgument(
                input == 0 || input == 1,
                "is either 0 or 1",
                "input"
            );
            ScaleOption ScaleChoice = (ScaleOption)input;
            Debug.Log("Setting scale choice to " + ScaleChoice.ToString());
            SetScaleInputsActive(CurrentScaleOption, ScaleChoice);
        }
        // We need the duplication/explicit overloading below
        // for it to appear in the Unity editor view :/

        public void SetLinearScale(String newValue)
            { CurrentLinearScale = ParseScale(newValue); }
        public void SetLinearScale(float newValue)
            { CurrentLinearScale = ParseScale(newValue); }
        public void SetLinearScale <T>(T newValue)
            { CurrentLinearScale = ParseScale(newValue); }

        public void SetLogBase(String newValue)
            { CurrentLogBase = ParseScale(newValue); }
        public void SetLogBase(float newValue)
            { CurrentLogBase = ParseScale(newValue); }
        public void SetLogBase<T>(T newValue)
            { CurrentLogBase = ParseScale(newValue); }

        public void SetLogInnerMultiplier(String newValue)
            { CurrentLogInnerMultiplier = ParseScale(newValue); }
        public void SetLogInnerMultiplier(float newValue)
            { CurrentLogInnerMultiplier = ParseScale(newValue); }
        public void SetLogInnerMultiplier<T>(T newValue)
            { CurrentLogInnerMultiplier = ParseScale(newValue); }

        public void SetLogOuterMultiplier(String newValue)
            { CurrentLogOuterMultiplier = ParseScale(newValue); }
        public void SetLogOuterMultiplier(float newValue)
            { CurrentLogOuterMultiplier = ParseScale(newValue); }
        public void SetLogOuterMultiplier<T>(T newValue)
            { CurrentLogOuterMultiplier = ParseScale(newValue); }

        public float ParseScale (String newValue)
            { return Convert.ToSingle(newValue); }
        public float ParseScale (float newValue)
            { return newValue; }
        public float ParseScale <NonspecificType> (NonspecificType newValue)
            { return Convert.ToSingle(newValue); }

        private void
        SetScaleInputsActive
            (ScaleOption currentActiveOption,
             ScaleOption desiredActiveOption)
        {
            SetScaleInputsActive(currentActiveOption, false);
            SetScaleInputsActive(desiredActiveOption, true);
            CurrentScaleOption = desiredActiveOption;
        }

        private void
        SetScaleInputsActive
            (ScaleOption targetObjects, bool active)
        {
            switch (targetObjects)
            {
                case ScaleOption.Linear:
                    SetObjectsActive(LinearScaleOptions, active);
                    break;
                case ScaleOption.Logarithmic:
                    SetObjectsActive(LogarithmicScaleOptions, active);
                    break;
                default:
                    throw new UnexpectedEnumValueException
                        <ScaleOption>(targetObjects);
            }

            CurrentScaleType = targetObjects;
        }

        private void SetObjectsActive (List<GameObject> gameObjects, bool active)
        {
            foreach (GameObject go in gameObjects)
            {
                go.SetActive(active);
            }
        }
    }
}
