using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    public abstract class RegistryModule <RegisteredType>
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

        protected Dictionary<int, RegisteredType> RegisteredObjects
            = new Dictionary<int, RegisteredType>();

        // -- Delegates --

        protected delegate
            void InitialisationDelegate
                (RegisteredType objectToInitialise, int index);

        // -- Properties --
        public RegisteredType this[int index]
        {
            get
            {
                return RetrieveObject(index);
            }
        }

        // -- Methods --
        public abstract void EnsureObjectsStayAlive();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefabs">
        /// N.B. Each prefab needs to have an IGameObjectRegistryKeyComponent
        /// MonoBehaviour/module attached.
        /// </param>
        public void
        InitialiseAndRegisterGenericPrefabs
            (List<RegisteredType> prefabs)
        {
            GenericRegisterFromList(prefabs, true, null);
        }

        /// <summary>
        /// Expects GameObjects which have already been instantiated
        /// e.g. ones which are part of the same prefab as the parent.
        /// </summary>
        /// <param name="gameObjects">
        /// </param>
        /// <param name="register"></param>
        public void
        RegisterObjects
            (List<RegisteredType> gameObjects)
        {
            GenericRegisterFromList(
                gameObjects,
                false,
                null
            );
        }

        public RegisteredType RetrieveObject(int element)
        {
            RegisteredType obj;
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
        public bool TryGetValue(int key, out RegisteredType value)
        {
            return RegisteredObjects.TryGetValue(key, out value);
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
            foreach (RegisteredType RegisteredObject in RegisteredObjects.Values)
            {
                ActivateGameObject(RegisteredObject, active);
            }
        }

        public void ActivateGameObject(int elementIdentifier, bool active)
        {
            RegisteredType obj;
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
                    ActivateGameObject(obj, active);
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
        
        public bool Contains (int key)
        {
            return RegisteredObjects.ContainsKey(key);
        }

        public int Count ()
        {
            return RegisteredObjects.Count;
        }

        protected abstract void
        ActivateGameObject
            (RegisteredType registeredObject,
             bool active);

        protected abstract void
        GenericRegisterObject
            (RegisteredType objectToRegister,
            bool objectIsAPrefab,
            InitialisationDelegate initialisationCallback,
            int Index);

        /// <summary>
        /// The big one - contains all the shared logic
        /// </summary>
        /// <param name="objectsToRegister"></param>
        /// <param name="objectsArePrefabs"></param>
        /// <param name="initialisationCallback"></param>
        protected void GenericRegisterFromList
            (List<RegisteredType> objectsToRegister,
             bool objectsArePrefabs,
             InitialisationDelegate initialisationCallback)
        {
            for (int Index = 0; Index < objectsToRegister.Count; Index++)
            {
                RegisteredType OriginalObject = objectsToRegister[Index];
                MyContract.RequireArgumentNotNull(
                    OriginalObject,
                    "Provided object at index " + Index
                );
                GenericRegisterObject(OriginalObject, objectsArePrefabs, initialisationCallback, Index);
            }
            //Debug.Log("Registered indices: " + PrintRegistry());
        }

        protected string PrintRegistry ()
        {
            string ReturnString = "[";
            foreach (KeyValuePair<int, RegisteredType> entry in RegisteredObjects)
            {
                ReturnString += "(";
                ReturnString += PrintKey(entry.Key);
                ReturnString += "),";
            }
            ReturnString += "]";
            return ReturnString;
        }

        protected string PrintKey (int key)
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
        
        // TODO: Remove once debug complete
        public string PrintDebugDestroyedRegisteredObjectCheck ()
        {
            string ReturnString = "[";
            foreach (KeyValuePair<int, RegisteredType> entry in RegisteredObjects)
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

        /// <summary>
        /// Only valid to use where the key int represents a flag enum
        /// </summary>
        /// <returns></returns>
        public HashSet<int> GenerateRegisteredObjectFlags ()
        {
            HashSet<int> Flags = new HashSet<int>();
            foreach (int key in RegisteredObjects.Keys)
            {
                Flags.Add(key);
            }
            return Flags;
        }
    }
}

