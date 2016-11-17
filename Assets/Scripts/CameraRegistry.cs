using System;
using UnityEngine;

namespace SpaceBattles
{
    public class CameraRegistry : RegistryModule<Camera>
    {
        //SetCamerasFollowTransform

        public override void EnsureObjectsStayAlive()
        {
            if (!PersistThroughScenes)
            {
                Debug.LogWarning("PersistThroughScenes was false");
                PersistThroughScenes = true;
            }

            foreach (Camera RegisteredCamera in RegisteredObjects.Values)
            {
                GameObject.DontDestroyOnLoad(RegisteredCamera);
                GameObject.DontDestroyOnLoad(RegisteredCamera.gameObject);
            }
        }

        protected override void ActivateGameObject(Camera registeredObject, bool active)
        {
            registeredObject.gameObject.SetActive(active);
        }

        protected override void
        GenericRegisterObject
            (Camera objectToRegister,
            bool objectIsAPrefab,
            InitialisationDelegate initialisationCallback,
            int Index)
        {
            Camera Instance;
            if (objectIsAPrefab)
            {
                GameObject Prefab = objectToRegister.gameObject;
                GameObject Obj = GameObject.Instantiate(Prefab);
                Instance = Obj.GetComponent<Camera>();
                MyContract.RequireArgumentNotNull(Instance, "Camera Component");
            }
            else
            {
                Instance = objectToRegister;
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
                GameObject.DontDestroyOnLoad(Instance.gameObject);
            }
        }
    }
}