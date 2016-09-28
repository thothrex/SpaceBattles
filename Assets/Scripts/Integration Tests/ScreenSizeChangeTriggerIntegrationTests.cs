using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SpaceBattles
{
    public class ScreenSizeChangeTriggerIntegrationTests : MonoBehaviour
    {
        public void Start()
        {
            RectResizeTriggerTest();
            IntegrationTest.Pass(gameObject);
        }
        
        /// <summary>
        /// Needs the test object in the integration test scene
        /// to already have a recttransform and ScreenSiseChangeTrigger
        /// attached (& initialised)
        /// </summary>
        public void RectResizeTriggerTest()
        {
            var confirmation_dict
                = new Dictionary<string, Dictionary<float, bool>>();
            var breakpoints_to_validate
                = new SortedList<float, ScreenSizeChangeLogic.ScreenBreakpointHandler>
                    (new FloatInverseOrderAllowDuplicatesComparer());
            float test_breakpoint = 20.0f;
            breakpoints_to_validate.Add(
                test_breakpoint,
                generateHandlerForGivenDict("A", test_breakpoint, confirmation_dict)
            );
            var registrant_a = new GameObject();

            var test_obj = gameObject;
            RectTransform rect = GetComponent<RectTransform>();
            Assert.IsNotNull(rect);
            ScreenSizeChangeTrigger test_trigger = GetComponent<ScreenSizeChangeTrigger>();
            Assert.IsNotNull(test_trigger);
            test_trigger.registerWidthBreakpointHandlers(breakpoints_to_validate, registrant_a);

            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 10.0f);
            
            Assert.IsTrue(confirmation_dict.ContainsKey("A"));
            Assert.IsTrue(confirmation_dict["A"][test_breakpoint]);
        }

        /// <summary>
        /// Copy pasted from ScreenSizeChangeTriggerUnitTests
        /// but there's no need for the two to have the same behaviour
        /// so they are copied here to allow for divergence as necessary
        /// </summary>
        /// <param name="object_name"></param>
        /// <param name="breakpoint"></param>
        /// <param name="confirmation_dict"></param>
        /// <returns></returns>
        private ScreenSizeChangeLogic.ScreenBreakpointHandler
            generateHandlerForGivenDict(string object_name,
                                        float breakpoint,
                                        Dictionary<string, Dictionary<float, bool>> confirmation_dict)
        {
            Debug.Log(handlerCreated(object_name, breakpoint));
            return delegate () {
                if (!confirmation_dict.ContainsKey(object_name))
                {
                    confirmation_dict.Add(object_name, new Dictionary<float, bool>());
                }
                confirmation_dict[object_name][breakpoint] = true;
            };
        }

        /// <summary>
        /// Copy pasted from ScreenSizeChangeTriggerUnitTests
        /// but there's no need for the two to have the same behaviour
        /// so they are copied here to allow for divergence as necessary
        /// </summary>
        /// <param name="object_name"></param>
        /// <param name="breakpoint"></param>
        /// <param name="confirmation_dict"></param>
        /// <returns></returns>
        private string handlerCreated(string obj_name, float breakpoint)
        {
            return "Handler created for object " + obj_name
                 + " for breakpoint " + breakpoint.ToString();
        }
    }
}