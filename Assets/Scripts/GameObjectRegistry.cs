using System;
using UnityEngine;

namespace SpaceBattles
{
    public class GameObjectRegistry : RegistryModule<GameObject>
    {
        public override void EnsureObjectsStayAlive()
        {
            if (!PersistThroughScenes)
            {
                Debug.LogWarning("PersistThroughScenes was false");
                PersistThroughScenes = true;
            }

            foreach (GameObject RegisteredObject in RegisteredObjects.Values)
            {
                GameObject.DontDestroyOnLoad(RegisteredObject);
            }
        }

        protected override void
        ActivateGameObject
            (GameObject registeredObject,
            bool active)
        {
            registeredObject.SetActive(active);
        }

        protected override void
        GenericRegisterObject
            (GameObject originalObject,
            bool objectIsAPrefab,
            InitialisationDelegate initialisationCallback,
            int Index)
        {
            GameObject Instance;
            if (objectIsAPrefab)
            {
                GameObject Prefab = originalObject;
                Instance = GameObject.Instantiate(Prefab);
                Debug.Assert(Instance != null, "Instantiate returned null");
            }
            else
            {
                Instance = originalObject;
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
            && RetrieveObject(element) != null)
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
    }
}
