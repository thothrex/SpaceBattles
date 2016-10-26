using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    public class GameObjectRegistryModule
    {
        // -- Constant Fields --
        private const string GAME_OBJECT_RETRIEVAL_ERRMSG_P1
            = "GameObject with element identifier ";
        private const string GAME_OBJECT_RETRIEVAL_ERRMSG_P2
            = " has not been initialised, but it is being accessed.";

        // -- Fields --
        private Dictionary<int, GameObject> RegisteredObjects
            = new Dictionary<int, GameObject>();

        // -- Delegates -- 
        private delegate
            void InitialisationDelegate
                (GameObject objectToInitialise, int index);

        // -- Properties --
        public GameObject this[int index]
        {
            get
            {
                return RetrieveGameObject(index);
            }
        }

        // -- Methods --
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
        /// 
        /// </summary>
        /// <param name="prefabs">
        /// N.B. Each prefab needs to have an IGameObjectRegistryKeyComponent
        /// MonoBehaviour/module attached.
        /// </param>
        public void
        InitialiseAndRegisterGenericPrefabs
            (List<GameObject> prefabs)
        {
            GenericRegisterFromList(prefabs, true, null);
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
            (List<GameObject>gameObjects,
             IScreenSizeBreakpointRegister register)
        {
            GenericRegisterFromList(
                gameObjects,
                false,
                CreateBreakpointRegistrationCallback(register)
            );
        }

        public GameObject RetrieveGameObject(int element)
        {
            GameObject obj;
            if (RegisteredObjects.TryGetValue(element, out obj))
            {
                return obj;
            }
            else
            {
                string err_msg = GAME_OBJECT_RETRIEVAL_ERRMSG_P1
                               + element.ToString()
                               + GAME_OBJECT_RETRIEVAL_ERRMSG_P2;
                throw new InvalidOperationException(err_msg);
            }
        }

        /// <summary>
        /// Just a passthrough for Dictionary.TryGetValue
        /// i.e. still used semantically for cases where returning a value
        /// is not necessary
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(int key, out GameObject value)
        {
            return RegisteredObjects.TryGetValue(key, out value);
        }

        public void ActivateGameObject(int elementIdentifier, bool active)
        {
            GameObject obj;
            if (RegisteredObjects.TryGetValue(elementIdentifier, out obj))
            {
                obj.SetActive(active);
            }
            else
            {
                string err_msg = "Target GameObject with element identifier "
                               + elementIdentifier
                               + " could not be found in this registry.";
                throw new InvalidOperationException(err_msg);
            }
        }

        public void ActivateGameObjects(bool active, params int[] bodiesToActivate)
        {
            if (bodiesToActivate.Length <= 0)
            {
                Debug.LogWarning("Redundant or broken registry activation call");
            }
            foreach (int elementIdentifier in bodiesToActivate)
            {
                ActivateGameObject(elementIdentifier, active);
            }
        }

        public bool Contains (int key)
        {
            return RegisteredObjects.ContainsKey(key);
        }

        public int Count ()
        {
            return RegisteredObjects.Count;
        }

        /// <summary>
        /// The big one - contains all the shared logic
        /// </summary>
        /// <param name="gameObjects"></param>
        /// <param name="objectsArePrefabs"></param>
        /// <param name="initialisationCallback"></param>
        private void GenericRegisterFromList
            (List<GameObject> gameObjects,
             bool objectsArePrefabs,
             InitialisationDelegate initialisationCallback)
        {
            for (int Index = 0; Index < gameObjects.Count; Index++)
            {
                GameObject OriginalObject = gameObjects[Index];
                GameObject Instance;
                if (objectsArePrefabs)
                {
                    Instance = GameObject.Instantiate(OriginalObject);
                }
                else
                {
                    Instance = OriginalObject;
                }

                if (initialisationCallback != null)
                {
                    initialisationCallback(Instance, Index);
                }

                IGameObjectRegistryKeyComponent KeyComponent
                    = Instance.GetComponent<IGameObjectRegistryKeyComponent>();
                int element = KeyComponent.Key;
                //Debug.Log("Adding element " + element.ToString() + " to the dictionary.");
                if (RegisteredObjects.ContainsKey(element)
                && RetrieveGameObject(element) != null)
                {
                    throw new InvalidOperationException(
                        "Trying to register a second GameObject "
                        + " with element identifier "
                        + element.ToString()
                    );
                }
                else
                {
                    RegisteredObjects.Add(element, Instance);
                }
            }
            Debug.Log("Registered indices: " + PrintRegistry());
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
                    +   NewObj
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
                Debug.Log("Registering breakpoints for element with elementid "
                         + UICS.ElementIdentifier);
            };
        }

        private string PrintRegistry ()
        {
            string ReturnString = "[";
            foreach (KeyValuePair<int, GameObject> entry in RegisteredObjects)
            {
                ReturnString += "(";
                ReturnString += entry.Key;
                ReturnString += "),";
            }
            ReturnString += "]";
            return ReturnString;
        }
    }
}

