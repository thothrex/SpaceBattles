using UnityEngine;
using UnityEditor;
using System;
using NUnit.Framework;
using System.Collections.Generic;

// typedef
using ObjectTriggers
    = System.Collections.Generic.SortedList
        <float, SpaceBattles.ScreenSizeChangeLogic.ScreenBreakpointHandler>;
using System.Linq;

namespace SpaceBattles
{
    public class ScreenSizeChangeTriggerUnitTests
    {
        private const string MISSING_NAME_EXC
            = "Mising a name for the object corresponding to the following "
            + "object triggers: ";
        private const string MISSING_TRIGGER_OBJECT_EXCEPTION
            = "Missing a registrant object for the following "
            + "object triggers: ";
        private delegate void ObjectBreakpointTriggeredHandler(float breakpoint);
        private enum DimensionToTest { WIDTH, HEIGHT };

        

        [Test]
        public void OneObjectOneWidthBreakpointTest()
        {
            var test_logic = new ScreenSizeChangeLogic();
            var breakpoints_to_validate
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var breakpoints_to_exclude
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var confirmation_dict
                = new Dictionary<string, Dictionary<float, bool>>();
            var object_names
                = new Dictionary<ObjectTriggers, string>();
            var registrant_objects
                = new Dictionary<ObjectTriggers, object>();

            var object_a_triggers
                = createObjectBreakpoints("A", confirmation_dict, 100.0f);
            breakpoints_to_validate.Add(object_a_triggers.Keys.Max(), object_a_triggers);
            object_names.Add(object_a_triggers, "A");
            var registrant_object_a = new GameObject();
            registrant_objects.Add(object_a_triggers, registrant_object_a);

            doDimensionBreakpointTest(test_logic,
                                      breakpoints_to_validate,
                                      breakpoints_to_exclude,
                                      confirmation_dict,
                                      object_names,
                                      registrant_objects,
                                      DimensionToTest.WIDTH,
                                      50.0f);
        }

        [Test]
        public void OneObjectOneHeightBreakpointTest()
        {
            var test_logic = new ScreenSizeChangeLogic();
            var breakpoints_to_validate
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var breakpoints_to_exclude
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var confirmation_dict
                = new Dictionary<string, Dictionary<float, bool>>();
            var object_names
                = new Dictionary<ObjectTriggers, string>();
            var registrant_objects
                = new Dictionary<ObjectTriggers, object>();

            var object_a_triggers
                = createObjectBreakpoints("A", confirmation_dict, 100.0f);
            breakpoints_to_validate.Add(object_a_triggers.Keys.Max(), object_a_triggers);
            object_names.Add(object_a_triggers, "A");
            var registrant_object_a = new GameObject();
            registrant_objects.Add(object_a_triggers, registrant_object_a);

            doDimensionBreakpointTest(test_logic,
                                      breakpoints_to_validate,
                                      breakpoints_to_exclude,
                                      confirmation_dict,
                                      object_names,
                                      registrant_objects,
                                      DimensionToTest.HEIGHT,
                                      50.0f);
        }

        [Test]
        public void OneObjectOneWidthDoNotTriggerBreakpointTest()
        {
            var test_logic = new ScreenSizeChangeLogic();
            var breakpoints_to_validate
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var breakpoints_to_exclude
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var confirmation_dict
                = new Dictionary<string, Dictionary<float, bool>>();
            var object_names
                = new Dictionary<ObjectTriggers, string>();
            var registrant_objects
                = new Dictionary<ObjectTriggers, object>();

            var object_a_triggers
                = createObjectBreakpoints("A", confirmation_dict, 100.0f);
            breakpoints_to_exclude.Add(object_a_triggers.Keys.Max(), object_a_triggers);
            object_names.Add(object_a_triggers, "A");
            var registrant_object_a = new GameObject();
            registrant_objects.Add(object_a_triggers, registrant_object_a);

            doDimensionBreakpointTest(test_logic,
                                      breakpoints_to_validate,
                                      breakpoints_to_exclude,
                                      confirmation_dict,
                                      object_names,
                                      registrant_objects,
                                      DimensionToTest.WIDTH,
                                      150.0f);
        }

        [Test]
        public void OneObjectOneHeightDoNotTriggerBreakpointTest()
        {
            var test_logic = new ScreenSizeChangeLogic();
            var breakpoints_to_validate
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var breakpoints_to_exclude
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var confirmation_dict
                = new Dictionary<string, Dictionary<float, bool>>();
            var object_names
                = new Dictionary<ObjectTriggers, string>();
            var registrant_objects
                = new Dictionary<ObjectTriggers, object>();

            var object_a_triggers
                = createObjectBreakpoints("A", confirmation_dict, 100.0f);
            breakpoints_to_exclude.Add(object_a_triggers.Keys.Max(), object_a_triggers);
            object_names.Add(object_a_triggers, "A");
            var registrant_object_a = new GameObject();
            registrant_objects.Add(object_a_triggers, registrant_object_a);

            doDimensionBreakpointTest(test_logic,
                                      breakpoints_to_validate,
                                      breakpoints_to_exclude,
                                      confirmation_dict,
                                      object_names,
                                      registrant_objects,
                                      DimensionToTest.HEIGHT,
                                      150.0f);
        }

        [Test]
        public void OneObjectFourHeightBreakpointsTriggerLowestTest()
        {
            var test_logic = new ScreenSizeChangeLogic();
            var breakpoints_to_validate
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var breakpoints_to_exclude
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var confirmation_dict
                = new Dictionary<string, Dictionary<float, bool>>();
            var object_names
                = new Dictionary<ObjectTriggers, string>();
            var registrant_objects
                = new Dictionary<ObjectTriggers, object>();

            var object_a_triggers
                = createObjectBreakpoints("A",
                                          confirmation_dict,
                                          30.0f);
            var object_a_ignores
                = createObjectBreakpoints("A",
                                          confirmation_dict,
                                          40.0f,
                                          50.0f,
                                          60.0f);

            breakpoints_to_validate.Add(object_a_triggers.Keys.Max(), object_a_triggers);
            object_names.Add(object_a_triggers, "A");
            breakpoints_to_exclude.Add(object_a_ignores.Keys.Max(), object_a_ignores);
            object_names.Add(object_a_ignores, "A");
            var registrant_object_a = new GameObject();
            registrant_objects.Add(object_a_triggers, registrant_object_a);
            registrant_objects.Add(object_a_ignores, registrant_object_a);


            doDimensionBreakpointTest(test_logic,
                                      breakpoints_to_validate,
                                      breakpoints_to_exclude,
                                      confirmation_dict,
                                      object_names,
                                      registrant_objects,
                                      DimensionToTest.HEIGHT,
                                      20.0f);
        }

        [Test]
        public void OneObjectFourHeightBreakpointsTriggerMiddleTest()
        {
            var test_logic = new ScreenSizeChangeLogic();
            var breakpoints_to_validate
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var breakpoints_to_exclude
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var confirmation_dict
                = new Dictionary<string, Dictionary<float, bool>>();
            var object_names
                = new Dictionary<ObjectTriggers, string>();
            var registrant_objects
                = new Dictionary<ObjectTriggers, object>();

            var object_a_triggers
                = createObjectBreakpoints("A",
                                          confirmation_dict,
                                          40.0f);
            var object_a_ignores
                = createObjectBreakpoints("A",
                                          confirmation_dict,
                                          30.0f,
                                          50.0f,
                                          60.0f);

            breakpoints_to_validate.Add(object_a_triggers.Keys.Max(), object_a_triggers);
            object_names.Add(object_a_triggers, "A");
            breakpoints_to_exclude.Add(object_a_ignores.Keys.Max(), object_a_ignores);
            object_names.Add(object_a_ignores, "A");
            var registrant_object_a = new GameObject();
            registrant_objects.Add(object_a_triggers, registrant_object_a);
            registrant_objects.Add(object_a_ignores, registrant_object_a);


            doDimensionBreakpointTest(test_logic,
                                      breakpoints_to_validate,
                                      breakpoints_to_exclude,
                                      confirmation_dict,
                                      object_names,
                                      registrant_objects,
                                      DimensionToTest.HEIGHT,
                                      35.0f);
        }

        [Test]
        public void OneObjectFourHeightBreakpointsTriggerTopTest()
        {
            var test_logic = new ScreenSizeChangeLogic();
            var breakpoints_to_validate
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var breakpoints_to_exclude
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var confirmation_dict
                = new Dictionary<string, Dictionary<float, bool>>();
            var object_names
                = new Dictionary<ObjectTriggers, string>();
            var registrant_objects
                = new Dictionary<ObjectTriggers, object>();

            var object_a_triggers
                = createObjectBreakpoints("A",
                                          confirmation_dict,
                                          60.0f);
            var object_a_ignores
                = createObjectBreakpoints("A",
                                          confirmation_dict,
                                          30.0f,
                                          40.0f,
                                          50.0f);

            breakpoints_to_validate.Add(object_a_triggers.Keys.Max(), object_a_triggers);
            object_names.Add(object_a_triggers, "A");
            breakpoints_to_exclude.Add(object_a_ignores.Keys.Max(), object_a_ignores);
            object_names.Add(object_a_ignores, "A");
            var registrant_object_a = new GameObject();
            registrant_objects.Add(object_a_triggers, registrant_object_a);
            registrant_objects.Add(object_a_ignores, registrant_object_a);


            doDimensionBreakpointTest(test_logic,
                                      breakpoints_to_validate,
                                      breakpoints_to_exclude,
                                      confirmation_dict,
                                      object_names,
                                      registrant_objects,
                                      DimensionToTest.HEIGHT,
                                      59.0f);
        }

        [Test]
        public void OneObjectFourHeightDoNotTriggerBreakpointsTest()
        {
            var test_logic = new ScreenSizeChangeLogic();
            var breakpoints_to_validate
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var breakpoints_to_exclude
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var confirmation_dict
                = new Dictionary<string, Dictionary<float, bool>>();
            var object_names
                = new Dictionary<ObjectTriggers, string>();
            var registrant_objects
                = new Dictionary<ObjectTriggers, object>();
            var object_a_ignores
                = createObjectBreakpoints("A",
                                          confirmation_dict,
                                          30.0f,
                                          40.0f,
                                          50.0f,
                                          60.0f);
            
            breakpoints_to_exclude.Add(object_a_ignores.Keys.Max(), object_a_ignores);
            object_names.Add(object_a_ignores, "A");
            var registrant_object_a = new GameObject();
            registrant_objects.Add(object_a_ignores, registrant_object_a);


            doDimensionBreakpointTest(test_logic,
                                      breakpoints_to_validate,
                                      breakpoints_to_exclude,
                                      confirmation_dict,
                                      object_names,
                                      registrant_objects,
                                      DimensionToTest.HEIGHT,
                                      61.0f);
        }

        [Test]
        public void OneObjectFourHeightFourWidthBreakpointsTest()
        {
            var test_logic = new ScreenSizeChangeLogic();
            var height_breakpoints_to_validate
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var height_breakpoints_to_exclude
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var height_confirmation_dict
                = new Dictionary<string, Dictionary<float, bool>>();
            var object_names
                = new Dictionary<ObjectTriggers, string>();
            var height_registrant_objects
                = new Dictionary<ObjectTriggers, object>();
            var width_registrant_objects
                = new Dictionary<ObjectTriggers, object>();

            var object_a_triggers
                = createObjectBreakpoints("A",
                                          height_confirmation_dict,
                                          30.0f);
            height_breakpoints_to_validate.Add(object_a_triggers.Keys.Max(), object_a_triggers);
            object_names.Add(object_a_triggers, "A");

            var object_a_ignores
                = createObjectBreakpoints("A",
                                          height_confirmation_dict,
                                          40.0f,
                                          50.0f,
                                          60.0f);
            height_breakpoints_to_exclude.Add(object_a_ignores.Keys.Max(), object_a_ignores);
            object_names.Add(object_a_ignores, "A");

            var registrant_object_a = new GameObject();
            height_registrant_objects.Add(object_a_triggers, registrant_object_a);
            height_registrant_objects.Add(object_a_ignores, registrant_object_a);

            var width_breakpoints_to_validate
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var width_breakpoints_to_exclude
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var width_confirmation_dict
                = new Dictionary<string, Dictionary<float, bool>>();

            object_a_triggers
                = createObjectBreakpoints("A",
                                          width_confirmation_dict,
                                          70.0f);
            object_a_ignores
                = createObjectBreakpoints("A",
                                          width_confirmation_dict,
                                          80.0f,
                                          90.0f,
                                          100.0f);
            width_breakpoints_to_validate.Add(object_a_triggers.Keys.Max(), object_a_triggers);
            object_names.Add(object_a_triggers, "A");
            width_breakpoints_to_exclude.Add(object_a_ignores.Keys.Max(), object_a_ignores);
            object_names.Add(object_a_ignores, "A");
            width_registrant_objects.Add(object_a_triggers, registrant_object_a);
            width_registrant_objects.Add(object_a_ignores, registrant_object_a);

            doDimensionBreakpointTest(test_logic,
                                      height_breakpoints_to_validate,
                                      height_breakpoints_to_exclude,
                                      height_confirmation_dict,
                                      object_names,
                                      height_registrant_objects,
                                      DimensionToTest.HEIGHT,
                                      20.0f);
            doDimensionBreakpointTest(test_logic,
                                      width_breakpoints_to_validate,
                                      width_breakpoints_to_exclude,
                                      width_confirmation_dict,
                                      object_names,
                                      width_registrant_objects,
                                      DimensionToTest.WIDTH,
                                      10.0f);
        }

        [Test]
        public void OneObjectFourHeightFourWidthBreakpointsTriggerMiddlesTest()
        {
            var test_logic = new ScreenSizeChangeLogic();
            var height_breakpoints_to_validate
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var height_breakpoints_to_exclude
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var height_confirmation_dict
                = new Dictionary<string, Dictionary<float, bool>>();
            var object_names
                = new Dictionary<ObjectTriggers, string>();
            var height_registrant_objects
                = new Dictionary<ObjectTriggers, object>();
            var width_registrant_objects
                = new Dictionary<ObjectTriggers, object>();

            var object_a_triggers
                = createObjectBreakpoints("A",
                                          height_confirmation_dict,
                                          40.0f);
            height_breakpoints_to_validate.Add(object_a_triggers.Keys.Max(), object_a_triggers);
            object_names.Add(object_a_triggers, "A");

            var object_a_ignores
                = createObjectBreakpoints("A",
                                          height_confirmation_dict,
                                          30.0f,
                                          50.0f,
                                          60.0f);
            height_breakpoints_to_exclude.Add(object_a_ignores.Keys.Max(), object_a_ignores);
            object_names.Add(object_a_ignores, "A");

            var registrant_object_a = new GameObject();
            height_registrant_objects.Add(object_a_triggers, registrant_object_a);
            height_registrant_objects.Add(object_a_ignores, registrant_object_a);

            var width_breakpoints_to_validate
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var width_breakpoints_to_exclude
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var width_confirmation_dict
                = new Dictionary<string, Dictionary<float, bool>>();

            object_a_triggers
                = createObjectBreakpoints("A",
                                          width_confirmation_dict,
                                          90.0f);
            object_a_ignores
                = createObjectBreakpoints("A",
                                          width_confirmation_dict,
                                          70.0f,
                                          80.0f,
                                          100.0f);
            width_breakpoints_to_validate.Add(object_a_triggers.Keys.Max(), object_a_triggers);
            object_names.Add(object_a_triggers, "A");
            width_breakpoints_to_exclude.Add(object_a_ignores.Keys.Max(), object_a_ignores);
            object_names.Add(object_a_ignores, "A");
            width_registrant_objects.Add(object_a_triggers, registrant_object_a);
            width_registrant_objects.Add(object_a_ignores, registrant_object_a);

            doDimensionBreakpointTest(test_logic,
                                      height_breakpoints_to_validate,
                                      height_breakpoints_to_exclude,
                                      height_confirmation_dict,
                                      object_names,
                                      height_registrant_objects,
                                      DimensionToTest.HEIGHT,
                                      35.0f);
            doDimensionBreakpointTest(test_logic,
                                      width_breakpoints_to_validate,
                                      width_breakpoints_to_exclude,
                                      width_confirmation_dict,
                                      object_names,
                                      width_registrant_objects,
                                      DimensionToTest.WIDTH,
                                      85.0f);
        }

        [Test]
        public void ThreeObjectThreeHeightThreeWidthBreakpointsTest()
        {
            var test_logic = new ScreenSizeChangeLogic();
            var height_breakpoints_to_validate
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var height_breakpoints_to_exclude
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var height_confirmation_dict
                = new Dictionary<string, Dictionary<float, bool>>();
            var object_names
                = new Dictionary<ObjectTriggers, string>();
            var height_registrant_objects
                = new Dictionary<ObjectTriggers, object>();
            var width_registrant_objects
                = new Dictionary<ObjectTriggers, object>();

            var object_a_triggers
                = createObjectBreakpoints("A",
                                          height_confirmation_dict,
                                          30.0f);
            height_breakpoints_to_validate.Add(object_a_triggers.Keys.Max(), object_a_triggers);
            object_names.Add(object_a_triggers, "A");
            var object_a_ignores
                = createObjectBreakpoints("A",
                                          height_confirmation_dict,
                                          40.0f,
                                          50.0f);
            height_breakpoints_to_exclude.Add(object_a_ignores.Keys.Max(), object_a_ignores);
            object_names.Add(object_a_ignores, "A");
            var registrant_object_a = new GameObject();
            height_registrant_objects.Add(object_a_triggers, registrant_object_a);
            height_registrant_objects.Add(object_a_ignores, registrant_object_a);

            var object_b_triggers
                = createObjectBreakpoints("B",
                                          height_confirmation_dict,
                                          40.0f);
            height_breakpoints_to_validate.Add(object_b_triggers.Keys.Max(), object_b_triggers);
            object_names.Add(object_b_triggers, "B");
            var object_b_ignores
                = createObjectBreakpoints("B",
                                          height_confirmation_dict,
                                          50.0f,
                                          60.0f);
            height_breakpoints_to_exclude.Add(object_b_ignores.Keys.Max(), object_b_ignores);
            object_names.Add(object_b_ignores, "B");
            var registrant_object_b = new GameObject();
            height_registrant_objects.Add(object_b_triggers, registrant_object_b);
            height_registrant_objects.Add(object_b_ignores, registrant_object_b);

            var object_c_triggers
                = createObjectBreakpoints("C",
                                          height_confirmation_dict,
                                          50.0f);
            height_breakpoints_to_validate.Add(object_c_triggers.Keys.Max(), object_c_triggers);
            object_names.Add(object_c_triggers, "C");
            var object_c_ignores
                = createObjectBreakpoints("C",
                                          height_confirmation_dict,
                                          60.0f,
                                          70.0f);
            height_breakpoints_to_exclude.Add(object_c_ignores.Keys.Max(), object_c_ignores);
            object_names.Add(object_c_ignores, "C");
            var registrant_object_c = new GameObject();
            height_registrant_objects.Add(object_c_triggers, registrant_object_c);
            height_registrant_objects.Add(object_c_ignores, registrant_object_c);

            var width_breakpoints_to_validate
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var width_breakpoints_to_exclude
                = new SortedList<float, ObjectTriggers>(new FloatInverseOrderAllowDuplicatesComparer());
            var width_confirmation_dict
                = new Dictionary<string, Dictionary<float, bool>>();

            object_a_triggers
               = createObjectBreakpoints("A",
                                         width_confirmation_dict,
                                         60.0f);
            width_breakpoints_to_validate.Add(object_a_triggers.Keys.Max(), object_a_triggers);
            object_names.Add(object_a_triggers, "A");
            object_a_ignores
                = createObjectBreakpoints("A",
                                          width_confirmation_dict,
                                          70.0f,
                                          80.0f);
            width_breakpoints_to_exclude.Add(object_a_ignores.Keys.Max(), object_a_ignores);
            object_names.Add(object_a_ignores, "A");
            width_registrant_objects.Add(object_a_triggers, registrant_object_a);
            width_registrant_objects.Add(object_a_ignores, registrant_object_a);

            object_b_triggers
                = createObjectBreakpoints("B",
                                          width_confirmation_dict,
                                          70.0f);
            width_breakpoints_to_validate.Add(object_b_triggers.Keys.Max(), object_b_triggers);
            object_names.Add(object_b_triggers, "B");
            object_b_ignores
                = createObjectBreakpoints("B",
                                          width_confirmation_dict,
                                          80.0f,
                                          90.0f);
            width_breakpoints_to_exclude.Add(object_b_ignores.Keys.Max(), object_b_ignores);
            object_names.Add(object_b_ignores, "B");
            width_registrant_objects.Add(object_b_triggers, registrant_object_b);
            width_registrant_objects.Add(object_b_ignores, registrant_object_b);

            object_c_triggers
                = createObjectBreakpoints("C",
                                          width_confirmation_dict,
                                          80.0f);
            width_breakpoints_to_validate.Add(object_c_triggers.Keys.Max(), object_c_triggers);
            object_names.Add(object_c_triggers, "C");
            object_c_ignores
                = createObjectBreakpoints("C",
                                          width_confirmation_dict,
                                          90.0f,
                                          100.0f);
            width_breakpoints_to_exclude.Add(object_c_ignores.Keys.Max(), object_c_ignores);
            object_names.Add(object_c_ignores, "C");
            width_registrant_objects.Add(object_c_triggers, registrant_object_c);
            width_registrant_objects.Add(object_c_ignores, registrant_object_c);

            doDimensionBreakpointTest(test_logic,
                                      height_breakpoints_to_validate,
                                      height_breakpoints_to_exclude,
                                      height_confirmation_dict,
                                      object_names,
                                      height_registrant_objects,
                                      DimensionToTest.HEIGHT,
                                      20.0f);
            doDimensionBreakpointTest(test_logic,
                                      width_breakpoints_to_validate,
                                      width_breakpoints_to_exclude,
                                      width_confirmation_dict,
                                      object_names,
                                      width_registrant_objects,
                                      DimensionToTest.WIDTH,
                                      20.0f);
        }

        private void doDimensionBreakpointTest
            (ScreenSizeChangeLogic test_logic,
             SortedList<float, ObjectTriggers> breakpoints_to_validate,
             SortedList<float, ObjectTriggers> breakpoints_to_exclude,
             Dictionary<string, Dictionary<float, bool>> confirmation_dict,
             Dictionary<ObjectTriggers, string> trigger_obj_names,
             Dictionary<ObjectTriggers, object> trigger_objects,
             DimensionToTest dimension,
             float breakpoint_to_trigger)
        {
            var validation_dict
                = new Dictionary<string, Dictionary<float, bool>>();

            foreach (var obj_breakpoints in breakpoints_to_validate)
            {
                addBreakpointsToValidationDict(true,
                                               test_logic,
                                               dimension,
                                               obj_breakpoints.Value,
                                               validation_dict,
                                               trigger_obj_names,
                                               trigger_objects);
            }
            Debug.Log("validation dict after triggers added: ");
            Debug.Log(printConfirmationDict(validation_dict));

            foreach (var obj_breakpoints in breakpoints_to_exclude)
            {
                addBreakpointsToValidationDict(false,
                                               test_logic,
                                               dimension,
                                               obj_breakpoints.Value,
                                               validation_dict,
                                               trigger_obj_names,
                                               trigger_objects);
            }
            Debug.Log("validation dict after excludes added: ");
            Debug.Log(printConfirmationDict(validation_dict));
            Rect test_rect;
            switch (dimension)
            {
                case DimensionToTest.HEIGHT:
                    test_rect = new Rect(0, 0, float.MaxValue, breakpoint_to_trigger);
                    test_logic.screenSizeChangeHandler(test_rect);
                    break;
                case DimensionToTest.WIDTH:
                    test_rect = new Rect(0, 0, breakpoint_to_trigger, float.MaxValue);
                    test_logic.screenSizeChangeHandler(test_rect);
                    break;
                default:
                    throw new UnexpectedEnumValueException<DimensionToTest>(dimension);
            }

            Debug.Log("validation dict after test: ");
            Debug.Log(printConfirmationDict(validation_dict));
            Debug.Log("confirmation dict after test: ");
            Debug.Log(printConfirmationDict(confirmation_dict));

            // check that validation dictionary is the same as
            // the dictionary filled out during the tes.
            foreach (var obj_entry in validation_dict)
            {
                string obj_name = obj_entry.Key;
                Debug.Log("Checking values for object " + obj_name);
                Assert.IsTrue(confirmation_dict.ContainsKey(obj_name),
                              confirmationDictInsufficientlyModified(obj_name));
                foreach (var obj_breakpoint_confirmation in obj_entry.Value)
                {
                    float breakpoint = obj_breakpoint_confirmation.Key;
                    bool expected_trigger_state
                        = validation_dict[obj_name][breakpoint];
                    if (!confirmation_dict[obj_name].ContainsKey(breakpoint))
                    {
                        Assert.IsFalse(
                            expected_trigger_state,
                            confirmationDictInsufficientlyModified(obj_name, breakpoint)
                        );
                    }
                    else
                    {
                        bool actual_trigger_state
                                                = confirmation_dict[obj_name][breakpoint];
                        Assert.AreEqual(
                            expected_trigger_state,
                            actual_trigger_state,
                            triggerStateDifferentErrorMessage(obj_name,
                                                              breakpoint,
                                                              actual_trigger_state,
                                                              expected_trigger_state)
                        );
                    }
                }
                Debug.Log("Values for object " + obj_name + " are correct");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trigger_objects">
        /// The objects corresponding to each set of breakpoint triggers
        /// </param>
        /// <param name="obj_triggers">
        /// The set of breakpoint triggers for the object
        /// we are currently considering
        /// </param>
        /// <returns></returns>
        private void addBreakpointsToValidationDict
            (bool validate,
             ScreenSizeChangeLogic test_logic,
             DimensionToTest dimension,
             ObjectTriggers obj_triggers,
             Dictionary<string, Dictionary<float,bool>> validation_dict,
             Dictionary<ObjectTriggers, string> trigger_obj_names,
             Dictionary<ObjectTriggers, object> trigger_objects)
        {
            Assert.IsTrue(trigger_obj_names.ContainsKey(obj_triggers),
                          MISSING_NAME_EXC + obj_triggers.ToString());
            Assert.IsTrue(trigger_objects.ContainsKey(obj_triggers),
                          printMissingTriggerObjectException(trigger_objects, obj_triggers));
            object registrant = trigger_objects[obj_triggers];
            registerBreakpointHandlers(test_logic, registrant, obj_triggers, dimension);

            string obj_name = trigger_obj_names[obj_triggers];
            // This will already be set up when
            // setting up the do not include triggers
            if (!validation_dict.ContainsKey(obj_name))
            {
                Dictionary<float, bool> obj_validation_dict
                    = new Dictionary<float, bool>();
                validation_dict.Add(obj_name, obj_validation_dict);
            }
            
            foreach (float obj_breakpoint in obj_triggers.Keys)
            {
                validation_dict[obj_name][obj_breakpoint] = validate;
            }
        }

        private ObjectTriggers createObjectBreakpoints
            (string object_name,
             Dictionary<string, Dictionary<float, bool>> confirmation_dict,
             params float[] breakpoints)
        {
            // we use this to confirm that each breakpoint has been triggered
            // for each object correctly (as expected)

            // This will already have been set up
            // when adding the exclude triggers
            // after the include/verify ones
            if (!confirmation_dict.ContainsKey(object_name))
            {
                var object_confirm_dict
                    = new Dictionary<float, bool>();
                confirmation_dict.Add(object_name, object_confirm_dict);
            }
            
            var object_triggers
                = new ObjectTriggers(new FloatInverseOrderAllowDuplicatesComparer());

            foreach (float breakpoint in breakpoints)
            {
                object_triggers.Add(
                    breakpoint,
                    generateHandlerForGivenDict(object_name,
                                                breakpoint,
                                                confirmation_dict)
                );
            }

            return object_triggers;
        }

        private ScreenSizeChangeLogic.ScreenBreakpointHandler
            generateHandlerForGivenDict(string object_name,
                                        float breakpoint,
                                        Dictionary<string, Dictionary<float, bool>> confirmation_dict)
        {
            Debug.Log(handlerCreated(object_name, breakpoint));
            return delegate() {
                if (!confirmation_dict.ContainsKey(object_name))
                {
                    confirmation_dict.Add(object_name, new Dictionary<float, bool>());
                }
                confirmation_dict[object_name][breakpoint] = true;
            };
        }

        private void registerBreakpointHandlers
            (ScreenSizeChangeLogic test_obj,
             object registrant,
             ObjectTriggers object_triggers,
             DimensionToTest dimension)
        {
            switch (dimension)
            {
                case DimensionToTest.HEIGHT:
                    test_obj.registerHeightBreakpointHandlers(object_triggers, registrant);
                    break;
                case DimensionToTest.WIDTH:
                    test_obj.registerWidthBreakpointHandlers(object_triggers, registrant);
                    break;
            }
        }

        private string handlerCreated (string obj_name, float breakpoint)
        {
            return "Handler created for object " + obj_name
                 + " for breakpoint " + breakpoint.ToString();
        }

        private string confirmationDictInsufficientlyModified (string obj_name)
        {
            return "The confirmation dictionary did not contain an entry "
                 + "for the given object: " + obj_name;
        }

        private string confirmationDictInsufficientlyModified
            (string obj_name, float breakpoint)
        {
            return "The confirmation dictionary did not contain an entry"
                 + " for the given breakpoint: " + breakpoint.ToString()
                 + " for the object: " + obj_name;
        }

        private string triggerStateDifferentErrorMessage(string object_name,
                                                         float breakpoint,
                                                         bool actual_trigger_state,
                                                         bool expected_trigger_state)
        {
            return "Object " + object_name
                 + ", breakpoint " + breakpoint.ToString()
                 + " was expected to "
                 + (expected_trigger_state ? "have triggered" : "not trigger")
                 + " but it "
                 + (actual_trigger_state ? "did trigger" : "did not trigger")
                 + ".";
        }

        private string printConfirmationDict
            (Dictionary<string, Dictionary<float, bool>> dict)
        {
            string returnstring = "";
            foreach (var obj_triggers in dict)
            {
                returnstring += obj_triggers.Key.ToString();
                returnstring += " (";
                foreach (var breakpoint_trigger in obj_triggers.Value)
                {
                    returnstring += "[";
                    returnstring += breakpoint_trigger.Key.ToString();
                    returnstring += ": ";
                    returnstring += breakpoint_trigger.Value.ToString();
                    returnstring += "] ";
                }
                returnstring += ")\n";
            }
            return returnstring;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trigger_objects">
        /// The objects corresponding to each set of breakpoint triggers
        /// </param>
        /// <param name="object_triggers">
        /// The set of breakpoint triggers for the object
        /// we are currently considering
        /// </param>
        /// <returns></returns>
        private string printMissingTriggerObjectException
            (Dictionary<ObjectTriggers, object> trigger_objects,
             ObjectTriggers object_triggers)
        {
            string returnstring = "Dictionary does not contain this object's "
                                + "triggers.\nDictionary: ";
            foreach (var obj_trigs in trigger_objects)
            {
                returnstring += "\n(";
                foreach (var trigger in obj_trigs.Key)
                {
                    returnstring += "["
                                 + trigger.Key.ToString()
                                 + "]";
                }
                returnstring += ")";
            }
            returnstring += "\nThis object's triggers: ";
            returnstring += "(";
            foreach (var trigger in object_triggers)
            {
                returnstring += "["
                             + trigger.Key.ToString()
                             + "]";
            }
            returnstring += ")";

            return returnstring;
        }
    }
}
