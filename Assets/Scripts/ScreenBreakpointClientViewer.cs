#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpaceBattles
{
    [CustomEditor(typeof(ScreenBreakpointClient))]
    public class ScreenBreakpointClientViewer : Editor
    {
        private const string LISTENING_OBJECT_FIELD_NAME
            = "ListeningObject";
        private const BindingFlags METHODS_TO_SHOW_FLAGS
            = BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.DeclaredOnly;
        private const int DEFAULT_METHOD_INDEX = 0;

        private MethodSelectionEditorModule<BreakpointEntry> MSEM
            = new MethodSelectionEditorModule<BreakpointEntry>();

        private List<string> selection_labels = new List<string>();
        private List<MethodInfo> listener_methods = new List<MethodInfo>();
        private ScreenBreakpointClient sb_client = null;
        private MonoBehaviour listening_object = null;

        private GUIContent dimension_text = new GUIContent("Dimension");
        private GUIContent callback_text = new GUIContent("Callback");
        private GUIContent breakpoint_text = new GUIContent("Breakpoint");
        private GUIContent header_text = new GUIContent("Registered Breakpoints");
        private GUIContent minus_text = new GUIContent("-");
        private GUIContent plus_text = new GUIContent("+");
        private GUIContent listener_label = new GUIContent("Object To Trigger");

        private Stack<BreakpointEntry> items_to_delete
            = new Stack<BreakpointEntry>();
        private List<GUIContent> KeyLabels = null;

        /// <summary>
        /// This is called every time we swap back to this
        /// i.e. this will be re-entered while it's still active
        /// </summary>
        public void OnEnable()
        {
            Debug.Log("SBCV enabled");
            if (target != null)
            {
                sb_client = (ScreenBreakpointClient)target;
                listening_object = sb_client.ListeningObject;

                if (listening_object != null)
                {
                    if (!MSEM.IsInitialised)
                    {
                        MSEM.Initialise(OnBreakpointEntryUpdate);
                    }
                    MSEM.ProvideObjectForMethodsToBeInvokedUpon(listening_object);
                    selection_labels = MSEM.SelectionNames;
                }
            }
        }

        /// <summary>
        /// Because of Unity's input style, all of the sub-functions have to
        /// have side-effects in order to accept user input;
        /// every function which visualises a field or button also produces
        /// output for it.
        /// My understanding is that it's also idiomatic to reassign the
        /// variables produced alongside the reading, so that's where
        /// the side-effects come from.
        /// </summary>
        override
        public void OnInspectorGUI()
        {
            serializedObject.Update();

            //Debug.Log("Drawing ScreenBreakpointClient");

            EditorGUILayout.LabelField("LunaShroom Editor", EditorStyles.centeredGreyMiniLabel);
            renderScriptDisplay();
            renderListenerObjectSelector(sb_client, listening_object);
            displayBreakpointList(sb_client, selection_labels);
        }

        public void
        OnBreakpointEntryUpdate
        (BreakpointEntry entry, MemberInfo handlerInfo, string newHandlerFunctionName)
        {
            var new_handler = generateMethodCallback(handlerInfo);
            //Debug.Log("new handler generated");
            entry.handler = new_handler;
            entry.handler_function_name = newHandlerFunctionName;
            entry.callback_instance = listening_object;
            //Debug.Log("Changing entry " + entry.ToString());
            //Debug.Log("Adding handler function \"" + newHandlerFunctionName + "\"");
        }


        private void renderScriptDisplay()
        {
            MyStandardEditorUI.RenderScriptDisplay(sb_client);
        }

        private void renderListenerObjectSelector
            (ScreenBreakpointClient sb_client, MonoBehaviour listening_object)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(listener_label);
            MonoBehaviour new_listening_object
                = (MonoBehaviour)
                EditorGUILayout
                .ObjectField(listening_object, typeof(MonoBehaviour), true);

            // If we are just redrawing this element they can be equal
            // if they are literally exactly the same object
            // then don't reassign
            if (new_listening_object != this.listening_object)
            {
                // regardless of whether or not the new element is null
                // do the assignment
                // (null is valid - no object)
                this.listening_object = new_listening_object;
                sb_client.ListeningObject = new_listening_object;
                MSEM.ProvideObjectForMethodsToBeInvokedUpon(new_listening_object);
                selection_labels = MSEM.SelectionNames;
                EditorUtility.SetDirty(this);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void displayBreakpointList
            (ScreenBreakpointClient sb_client, List<string> method_names)
        {
            if (KeyLabels == null)
            {
                GUIContent[] Labels =
                {
                    dimension_text, breakpoint_text, callback_text
                };
                KeyLabels = new List<GUIContent>(Labels);
            }
            MyStandardEditorUI.RenderPreListHeaders(header_text, KeyLabels);

            GUILayoutOption max_button_width_opt = GUILayout.MaxWidth(50.0f);
            if (sb_client.BreakpointEntries != null)
            {
                foreach (BreakpointEntry entry in sb_client.BreakpointEntries)
                {
                    bool should_delete_element
                        = renderBreakpointEntry(entry, method_names, max_button_width_opt);
                    if (should_delete_element)
                    {
                        items_to_delete.Push(entry);
                    }
                }
            }
            while (items_to_delete.Count > 0)
            {
                BreakpointEntry item_to_delete = items_to_delete.Pop();
                sb_client.BreakpointEntries.Remove(item_to_delete);
                MSEM.RemoveEntry(item_to_delete);
            }
            EditorGUI.indentLevel--;

            bool should_add_element = MSEM.RenderListButtons();
            if (should_add_element)
            {
                BreakpointEntry new_entry = new BreakpointEntry();
                if (sb_client.BreakpointEntries == null)
                {
                    Debug.Log("sb_client not started yet (but why?). Starting it now");
                    sb_client.Start();
                }
                sb_client.BreakpointEntries.Add(new_entry);
                MSEM.SetMemberSelection(new_entry, DEFAULT_METHOD_INDEX);
                EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// Side-effects: changes the entry's values according to user input
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="method_names"></param>
        /// <param name="max_button_width_opt"></param>
        /// <returns></returns>
        private bool renderBreakpointEntry
            (BreakpointEntry entry,
            List<string> method_names,
            GUILayoutOption max_button_width_opt)
        {
            //Debug.Log("Drawing a breakpoint entry");
            Rect rect = EditorGUILayout.BeginHorizontal();
            entry.dimension = (Dimension)EditorGUILayout.EnumPopup(entry.dimension);
            entry.breakpoint = EditorGUILayout.FloatField(entry.breakpoint);
            // this happens when we are rendering a prefab element
            // i.e. the list has elements already but the viewer isn't set up
            int CurrentMethodIndex =
                MSEM.RetrieveEntrysMemberSelection(
                    entry,
                    entry.handler_function_name
                );

            int new_method_index
                = EditorGUILayout.Popup(
                    CurrentMethodIndex,
                    MSEM.SelectionNames.ToArray()
                  );
            MSEM.SetMemberSelection(entry, new_method_index);
            bool element_deleted = GUILayout.Button(minus_text, max_button_width_opt);
            EditorGUILayout.EndHorizontal();
            return element_deleted;
        }

        private ScreenSizeChangeLogic.ScreenBreakpointHandler
            generateMethodCallback(MemberInfo memberInfo)
        {
            MyContract.RequireArgument(
                memberInfo.MemberType == MemberTypes.Method,
                "is an instance of type MethodInfo",
                "memberInfo");
            MethodInfo MethodInfo
                = (MethodInfo)memberInfo;
            return delegate ()
            {
                MethodInfo.Invoke(listening_object, null);
            };
        }
    }
}

#endif