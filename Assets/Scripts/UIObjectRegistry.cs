using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    public class UIRegistry : GameObjectRegistry
    {
        public void
        InitialiseAndRegisterUiPrefabs
            (List<GameObject> prefabs,
             IScreenSizeBreakpointRegister register,
             Canvas parentCanvas)
        {
            InitialisationDelegate Callback
                = CreateFreshUIComponentSetupCallback(
                    parentCanvas.GetComponent<RectTransform>(),
                    prefabs,
                    CreateBreakpointRegistrationCallback(register)
                );
            GenericRegisterFromList(prefabs, true, Callback);
        }

        /// <summary>
        /// Expects GameObjects which have already been instantiated
        /// e.g. ones which are part of the same prefab as the parent.
        /// </summary>
        /// <param name="gameObjects">
        /// These must have UiComponentStem MonoBehaviours/modules attached
        /// </param>
        /// <param name="register"></param>
        public void
        RegisterUiGameObjects
            (List<GameObject> gameObjects,
             IScreenSizeBreakpointRegister register)
        {
            GenericRegisterFromList(
                gameObjects,
                false,
                CreateBreakpointRegistrationCallback(register)
            );
        }

        /// <summary>
        /// Instantiates, but also sets parent canvas
        /// and recentres UI component relative to that parent canvas
        /// </summary>
        /// <param name="prefabs">
        /// Prefabs the UI components were instantiated from
        /// </param>
        /// <param name="parentTransform">
        /// Transform which will be the parent of the instantiated GameObject
        /// </param>
        /// <returns></returns>
        private InitialisationDelegate
        CreateFreshUIComponentSetupCallback
            (Transform parentTransform,
             List<GameObject> prefabs,
             InitialisationDelegate breakpointRegistrationCallback)
        {
            return delegate (GameObject NewObj, int index)
            {
                Debug.Log(
                    "Setting up fresh UI component with elementid "
                    + NewObj
                      .GetComponent<UIComponentStem>()
                      .ElementIdentifier
                );
                RectTransform NewTransform
                    = NewObj.GetComponent<RectTransform>();
                RectTransform PrefabTransform
                    = prefabs[index].GetComponent<RectTransform>();

                MyContract.RequireArgumentNotNull(NewTransform,
                                                  "Prefab's RectTransform");
                MyContract.RequireArgumentNotNull(PrefabTransform,
                                                  "Prefab's RectTransform");

                NewTransform.SetParent(parentTransform, false);

                // don't know why but special case
                if (NewObj.GetComponent<UIComponentStem>()
                    .ElementIdentifier == UIElements.SettingsMenu)
                {
                    NewTransform.anchorMin = PrefabTransform.anchorMin;
                    NewTransform.anchorMax = PrefabTransform.anchorMax;
                    NewTransform.offsetMin = PrefabTransform.offsetMin;
                    NewTransform.offsetMax = PrefabTransform.offsetMax;
                }
                NewTransform.localScale = new Vector3(1, 1, 1);

                breakpointRegistrationCallback(NewObj, index);
            };
        }

        private InitialisationDelegate
        CreateBreakpointRegistrationCallback
            (IScreenSizeBreakpointRegister register)
        {
            return delegate (GameObject currentObject, int index)
            {
                // Copy into this scope to preserve?
                IScreenSizeBreakpointRegister MyRegister = register;
                UIComponentStem UICS
                    = currentObject.GetComponent<UIComponentStem>();
                MyContract.RequireArgument(
                    UICS != null,
                    "has a UIComponentStem MonoBehavior/module attached",
                    "currentObject"
                );
                UICS.RegisterBreakpoints(MyRegister);
                //Debug.Log("Registering breakpoints for element with elementid "
                //         + UICS.ElementIdentifier);
            };
        }
    }
}
