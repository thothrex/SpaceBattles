using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SpaceBattles
{
    public class ScreenSizeChangeManagerIntegrationTests : MonoBehaviour
    {
        public GameObject trigger_obj;
        public GameObject manager_obj;

        public void Start()
        {
            // TODO: fix this for the new program flow
            //       rect -> trigger -> manager -> camera -> listeners

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
            float test_breakpoint = 500.0f;
            int test_resize_value = 50;
            int test_target_resize_value = 400;
            var confirmation_dict
                = new Dictionary<string, Dictionary<float, bool>>();
            var breakpoints_to_validate
                = new SortedList<float, ScreenSizeChangeLogic.ScreenBreakpointHandler>
                    (new FloatInverseOrderAllowDuplicatesComparer());
            breakpoints_to_validate.Add(
                test_breakpoint,
                generateHandlerForGivenDict("A", test_breakpoint, confirmation_dict)
            );

            //  rect -> trigger -> manager -> camera -> listeners
            //  in this case the listener object is (kind of) the confirmation dict
            Assert.IsNotNull(trigger_obj);
            RectTransform test_rect = trigger_obj.GetComponent<RectTransform>();
            Assert.IsNotNull(test_rect);
            ScreenSizeChangeTrigger test_trigger
                = trigger_obj.GetComponent<ScreenSizeChangeTrigger>();
            Assert.IsNotNull(test_trigger);

            Assert.IsNotNull(manager_obj);
            ScreenSizeChangeManager test_manager
                = manager_obj.GetComponent<ScreenSizeChangeManager>();
            Assert.IsNotNull(test_manager);
            Camera test_cam = test_manager.FixedUICamera;
            Assert.IsNotNull(test_cam);

            Resolution cur_res = Screen.currentResolution;
            Screen.SetResolution(test_target_resize_value, cur_res.height, false);
            Debug.Log("Aspect Ratio: " + test_cam.aspect
                      + "\nOrthographicSize: " + test_cam.orthographicSize
                      + "\npixelWidth: " + test_cam.pixelWidth
                      + "\tpixelHeight: " + test_cam.pixelHeight);
            Assert.IsTrue(test_target_resize_value > test_cam.pixelWidth,
                "Expected: " + test_cam.pixelWidth
                + " < " + test_target_resize_value);
            Assert.IsTrue(test_cam.orthographic,
                          "Expected camera to be in orthographic mode");


            test_trigger.ScreenResized.AddListener(test_manager.OnScreenSizeChange);
            test_manager.registerWidthBreakpointHandlers(breakpoints_to_validate, confirmation_dict);

            test_rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, test_resize_value);
            
            Assert.IsTrue(confirmation_dict.ContainsKey("A"),
                          "Confirmation dict does not contain the key: " + "A");
            Assert.IsTrue(confirmation_dict["A"][test_breakpoint]);

            // reset
            Screen.SetResolution(cur_res.width, cur_res.height, false);
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