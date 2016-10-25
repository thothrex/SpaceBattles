using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    public class UiComponentRegistryModule
    {
        // -- Constant Fields --
        private const string UI_OBJ_GET_ERRMSG_P1
            = "UI Object ";
        private const string UI_OBJ_GET_ERRMSG_P2
            = " has not been initialised, but it is being accessed.";

        // -- Fields --
        private Dictionary<UIElements, GameObject> UiComponentObjects
            = new Dictionary<UIElements, GameObject>();

        // -- Methods --
        public void
        InitialiseAndRegisterPrefabs
            (List<GameObject> prefabs,
             IScreenSizeBreakpointRegister register,
             Canvas parentCanvas)
        {
            foreach (GameObject Prefab in prefabs)
            {
                GameObject instance
                    = SetupUIComponentFromPrefab(Prefab, parentCanvas.transform);
                UIComponentStem stem_script = instance.GetComponent<UIComponentStem>();
                stem_script.RegisterBreakpoints(register);
                UIElements element = stem_script.ElementIdentifier;
                //Debug.Log("Adding element " + element.ToString() + " to the dictionary.");
                if (UiComponentObjects.ContainsKey(element)
                && RetrieveGameObject(element) != null)
                {
                    throw new InvalidOperationException(
                        "Trying to instantiate a second " + element.ToString()
                    );
                }
                else
                {
                    UiComponentObjects.Add(element, instance);
                }
            }
        }

        public GameObject RetrieveGameObject(UIElements element)
        {
            GameObject obj;
            if (UiComponentObjects.TryGetValue(element, out obj))
            {
                return obj;
            }
            else
            {
                string err_msg = UI_OBJ_GET_ERRMSG_P1
                               + element.ToString()
                               + UI_OBJ_GET_ERRMSG_P2;
                throw new InvalidOperationException(err_msg);
            }
        }

        /// <summary>
        /// Just a passtrhough for Dictionary.TryGetValue
        /// i.e. still used semantically for cases where returning a value
        /// is not necessary
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue (UIElements key, out GameObject value)
        {
            return UiComponentObjects.TryGetValue(key, out value);
        }

        /// <summary>
        /// Instantiates, but also sets parent canvas
        /// and recentres UI component relative to that parent canvas
        /// </summary>
        /// <param name="prefab">
        /// Prefab to instantiate the UI component from
        /// </param>
        /// <param name="parentTransform">
        /// Transform which will be the parent of the instantiated GameObject
        /// </param>
        /// <returns></returns>
        private GameObject SetupUIComponentFromPrefab(GameObject prefab, Transform parentTransform)
        {
            GameObject NewObj = GameObject.Instantiate(prefab);
            RectTransform NewTransform = NewObj.GetComponent<RectTransform>();
            RectTransform PrefabTransform = prefab.GetComponent<RectTransform>();

            MyContract.RequireArgumentNotNull(NewTransform, "Prefab's RectTransform");
            MyContract.RequireArgumentNotNull(PrefabTransform, "Prefab's RectTransform");

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
            return NewObj;
        }
    }
}

