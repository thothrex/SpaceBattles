using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    public class CameraRegistry : RegistryModule<Camera>
    {
        public MonoBehaviour CoroutineHost;

        private delegate void
            FadeFunction
                (CameraFader fader, Action partialFadeCallback);

        // -- Properties --
        public Camera this[CameraRoles index]
        {
            get
            {
                return RetrieveObject((int)index);
            }
        }

        // -- Methods --
        public void Fade (int cameraKey, bool fadeOut, Action fadeCallback)
        {
            Camera Cam = RegisteredObjects[cameraKey];
            CameraFader Fader = Cam.GetComponent<CameraFader>();
            MyContract.RequireFieldNotNull(
                Fader,
                "Camera " + cameraKey + "\'s Fader"
            );
            if (fadeOut)
            {
                Fader.FadeToBlack(fadeCallback);
            }
            else
            {
                Fader.FadeToClear(fadeCallback);
            }
        }

        public void FadeAllToBlack (Action fadeCallback)
        {
            FadeAll((c, a) => c.FadeToBlack(a), fadeCallback);
        }

        public void FadeAllToClear(Action fadeCallback)
        {
            FadeAll((c, a) => c.FadeToClear(a), fadeCallback);
        }

        public void SetAllFollowTransforms (Transform followTransform)
        {
            foreach (Camera Cam in RegisteredObjects.Values)
            {
                InertialCameraController ICC
                    = Cam.GetComponent<InertialCameraController>();
                if (ICC != null)
                {
                    ICC.FollowTransform = followTransform;
                }
                else
                {
                    LargeScaleCamera LSC
                        = Cam.GetComponent<LargeScaleCamera>();
                    if (LSC != null)
                    {
                        LSC.FollowTransform = followTransform;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "Camera "
                            + Cam.ToString()
                            + " does not have an InertialCameraController "
                            + "or LargeScaleCamera component"
                        );
                    }
                }
            }
        }

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

        public bool Contains (CameraRoles role)
        {
            return Contains((int)role);
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
            else if (RegisteredObjects.ContainsKey(element)
                 &&  RetrieveObject(element) == null)
            {
                // assume a deliberate replace intent
                RegisteredObjects[element] = Instance;
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


        private void FadeAll (FadeFunction fadeFunction, Action fadeCallback)
        {
            MyContract.RequireFieldNotNull(CoroutineHost, "CoroutineHost");
            HashSet<int> ObjectsToFade = new HashSet<int>();
            HashSet<int> FadedObjectsFlag = new HashSet<int>();
            foreach (var entry in RegisteredObjects)
            {
                Camera Cam = entry.Value;
                int Key = entry.Key;
                CameraFader FadeComponent
                    = Cam.gameObject.GetComponent<CameraFader>();
                
                if (FadeComponent != null)
                {
                    ObjectsToFade.Add(Key);
                    Action PartialFadeCallback = delegate ()
                    {
                        FadedObjectsFlag.Add(Key);
                    };
                    fadeFunction(FadeComponent, PartialFadeCallback);
                }
                else
                {
                    //Debug.Log("Camera "
                    //        + Cam.gameObject.name
                    //        + " has no Camera Fader - Skipping");
                }
            }
            if (fadeCallback != null)
            {
                CoroutineHost.StartCoroutine(
                    FadeCallbackCoroutine(
                        ObjectsToFade,
                        FadedObjectsFlag,
                        fadeCallback
                    )
                );
            }
        }

        private IEnumerator
        FadeCallbackCoroutine
            (HashSet<int> callbackChecklist,
             HashSet<int> completedCallbacks,
             Action fadeCompleteCallback)
        {
            yield return new WaitWhile(
                () => !callbackChecklist.SetEquals(completedCallbacks)
            );
            fadeCompleteCallback();
        }
    }
}