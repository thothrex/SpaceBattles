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
        public Type KeyEnum = null;

        /// <summary>
        /// N.B. Only applies to newly registered objects
        /// - will not update objects already in the registry
        /// </summary>
        public bool PersistThroughScenes = false;

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

        /// <summary>
        /// Expects GameObjects which have already been instantiated
        /// e.g. ones which are part of the same prefab as the parent.
        /// </summary>
        /// <param name="gameObjects">
        /// </param>
        /// <param name="register"></param>
        public void
        RegisterGameObjects
            (List<GameObject> gameObjects)
        {
            GenericRegisterFromList(
                gameObjects,
                false,
                null
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
                // Doesn't work for some reason?
                //MyContract.RequireFieldNotNull(
                //    obj,
                //    "Object registered to identifier " + elementIdentifier
                //);
                if (obj == null)
                {
                    if (RegisteredObjects.ContainsKey(elementIdentifier))
                    {
                        throw new InvalidOperationException(
                                "Object registered to identifier "
                                + PrintKey(elementIdentifier)
                                + " is null"
                            );
                    }
                    else // shouldn't ever fire
                    {
                        throw new InvalidOperationException(
                            "This registry does not contain an element with identifier "
                            + PrintKey(elementIdentifier)
                            + "\n"
                            + PrintRegistry()
                        );
                    }
                }
                else
                {
                    obj.SetActive(active);
                }
            }
            else
            {
                string err_msg = "Target GameObject with element identifier "
                               + PrintKey(elementIdentifier)
                               + " could not be found in this registry.";
                throw new InvalidOperationException(err_msg);
            }
        }

        public void
        ActivateGameObjectsFromIntFlag
            (bool active, int bodiesToActivate)
        {
            if (bodiesToActivate == 0) // corresponds to none for flags
                { return; }

            List<int> FlaggedBodiesToActivate = new List<int>();
            for (int CurrentFlagChecking = 1;
                 CurrentFlagChecking <= bodiesToActivate;
                 CurrentFlagChecking <<= 1)
            {
                if ((CurrentFlagChecking & bodiesToActivate) > 0)
                {
                    FlaggedBodiesToActivate.Add(CurrentFlagChecking);
                }
            }
            MyContract.RequireArgument(
                FlaggedBodiesToActivate.Count > 0,
                "corresponds to at least one registered GameObject",
                "bodiesToActivate"
            );
            ActivateGameObjects(active, FlaggedBodiesToActivate.ToArray());
        }

        public void
        ActivateGameObjects
            (bool active, params int[] bodiesToActivate)
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

        public void ActivateAllGameObjects(bool active)
        {
            foreach (GameObject RegisteredObject in RegisteredObjects.Values)
            {
                RegisteredObject.SetActive(active);
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
                    Debug.Assert(Instance != null, "Instantiate returned null");
                }
                else
                {
                    Instance = OriginalObject;
                    Debug.Assert(Instance != null, "Provided object was null");
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
                        + PrintKey(element)
                    );
                }
                else
                {
                    RegisteredObjects.Add(element, Instance);
                }

                if (PersistThroughScenes)
                {
                    GameObject.DontDestroyOnLoad(Instance);
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
                //Debug.Log("Registering breakpoints for element with elementid "
                //         + UICS.ElementIdentifier);
            };
        }

        private string PrintRegistry ()
        {
            string ReturnString = "[";
            foreach (KeyValuePair<int, GameObject> entry in RegisteredObjects)
            {
                ReturnString += "(";
                ReturnString += PrintKey(entry.Key);
                ReturnString += "),";
            }
            ReturnString += "]";
            return ReturnString;
        }

        private string PrintKey (int key)
        {
            if (KeyEnum != null && KeyEnum.IsEnum)
            {
                return Enum.GetName(KeyEnum, key);
            }
            else
            {
                return key.ToString();
            }
        }

        // TODO: make private after printstring debugging finished
        public string PrintDebugDestroyedRegisteredObjectCheck ()
        {
            string ReturnString = "[";
            foreach (KeyValuePair<int, GameObject> entry in RegisteredObjects)
            {
                ReturnString += "(";
                ReturnString += PrintKey(entry.Key);
                ReturnString += ", ";
                ReturnString += (entry.Value == null ? "null" : "not null");
                ReturnString += "),";
            }
            ReturnString += "]";
            return ReturnString;
        }
    }
}

