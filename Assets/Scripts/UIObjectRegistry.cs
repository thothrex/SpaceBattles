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

        public void
        InitialiseAndRegisterUiPrefabs
            (List<GameObject> prefabs,
             IScreenSizeBreakpointRegister register,
             RectTransform parentTransform)
        {
            InitialisationDelegate Callback
                = CreateFreshUIComponentSetupCallback(
                    parentTransform,
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

        public ManagerType
        RetrieveManager<ManagerType>
            (UIElements element)
        where ManagerType : Component
        {
            Type ExpectedManagerType = element.ManagerClass();
            MyContract.RequireArgument(
                typeof(ManagerType) == ExpectedManagerType,
                "is expected type",
                "ManagerType: " + typeof(ManagerType).Name
            );

            MyContract.RequireField
           (
               this.Contains(element),
               "contains the element " + element.ToString(),
               "RegisteredObjects"
            );

            ManagerType Manager
                = RegisteredObjects[(int)element].GetComponent<ManagerType>();
            MyContract.RequireFieldNotNull(
                Manager,
                typeof(ManagerType).Name
                    + " component of GameObject "
                    + element.ToString()
            );
            return Manager;
        }

        public void RegisterTransitions (UIManager uiManager)
        {
            foreach (GameObject Element in RegisteredObjects.Values)
            {
                ITransitionRequestBroadcaster TransitionBroadcaster
                    = Element.GetComponent<UIComponentStem>();
                MyContract.RequireFieldNotNull(
                    TransitionBroadcaster,
                    "TransitionBroadcaster"
                );
                uiManager.RegisterTransitionHandlers(TransitionBroadcaster);
            }
        }

        public bool Contains(UIElements key)
        {
            return base.Contains((int)key);
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
                //Debug.Log(
                //    "Setting up fresh UI component with elementid "
                //    + NewObj
                //      .GetComponent<UIComponentStem>()
                //      .ElementIdentifier
                //);
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
                UIElements NewElementId
                    = NewObj
                    .GetComponent<UIComponentStem>()
                    .ElementIdentifier;
                if (NewElementId == UIElements.SettingsMenu
                ||  NewElementId == UIElements.Scoreboard)
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
