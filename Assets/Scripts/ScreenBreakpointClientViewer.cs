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

        private List<string> selection_labels = new List<string>();
        private List<MethodInfo> listener_methods = new List<MethodInfo>();
        private Dictionary<BreakpointEntry, int>
            selected_method_index = new Dictionary<BreakpointEntry, int>();
        private ScreenBreakpointClient sb_client = null;
        private MonoBehaviour listening_object   = null;
        private Type listening_object_type;

        private GUILayoutOption min_width_opt    = GUILayout.MaxWidth(100.0f);
        private GUILayoutOption expand_width_opt = GUILayout.ExpandWidth(true);

        private GUIContent dimension_text  = new GUIContent("Dimension");
        private GUIContent callback_text   = new GUIContent("Callback");
        private GUIContent breakpoint_text = new GUIContent("Breakpoint");
        private GUIContent header_text     = new GUIContent("Registered Breakpoints");
        private GUIContent minus_text      = new GUIContent("-");
        private GUIContent plus_text       = new GUIContent("+");
        private GUIContent listener_label  = new GUIContent("Object To Trigger");
        private GUIContent script_label    = new GUIContent("Script");

        private Stack<BreakpointEntry> items_to_delete
            = new Stack<BreakpointEntry>();

        GUIStyle italic_style = null;

        public void OnEnable()
        {
            if (target != null)
            {
                sb_client = (ScreenBreakpointClient)target;
                listening_object = sb_client.ListeningObject;
                if (listening_object != null)
                {
                    listening_object_type = listening_object.GetType();
                    selection_labels
                        = retrieveMethodsAndGenerateGUILabels(listening_object_type, listening_object);
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
            //Debug.Log("Drawing ScreenBreakpointClient");
            if (italic_style == null)
            {
                italic_style = new GUIStyle(GUI.skin.label);
                italic_style.fontStyle = FontStyle.Italic;
            }

            EditorGUILayout.LabelField("LunaShroom Editor", EditorStyles.centeredGreyMiniLabel);
            renderScriptDisplay();
            renderListenerObjectSelector(sb_client, listening_object);
            displayBreakpointList(sb_client, selection_labels);
        }

        
        private void renderScriptDisplay()
        {
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = false;
            EditorGUILayout.PrefixLabel(script_label);
            EditorGUILayout.ObjectField(
                MonoScript.FromMonoBehaviour(sb_client),
                typeof(MonoBehaviour),
                true
            );
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
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
                if (new_listening_object != null)
                {
                    Debug.Log("Setting new listener object");
                    if (new_listening_object.GetType() != listening_object_type)
                    {
                        Debug.Log("Getting new type's new methods");
                        listening_object_type = new_listening_object.GetType();
                        selection_labels
                           = retrieveMethodsAndGenerateGUILabels(listening_object_type, new_listening_object);
                    }
                    // if they are the same type then don't bother
                    // recalculating the method labels
                }
                else
                {
                    Debug.Log("Setting new listener object to none: setting selection methods to empty");
                    listening_object_type = null;
                    selection_labels
                           = retrieveMethodsAndGenerateGUILabels(listening_object_type, new_listening_object);
                }
                EditorUtility.SetDirty(this);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void displayBreakpointList
            (ScreenBreakpointClient sb_client, List<string> method_names)
        {

            int num_breakpoints = 0;
            if (sb_client.BreakpointEntries != null)
            {
                num_breakpoints = sb_client.BreakpointEntries.Count;
            }

            // pre-list setup
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(header_text);

            EditorGUI.indentLevel++;
            // key as in the key to the table,
            // as in the name of each of the columns
            Rect key_rect = EditorGUILayout.BeginHorizontal();
            float max_width = key_rect.width / 4.0f;
            GUILayoutOption max_width_opt = GUILayout.MaxWidth(max_width);
            GUILayoutOption[] options = {
                max_width_opt, min_width_opt, expand_width_opt
            };
            //Debug.Log("max width recorded as " + max_width);
            EditorGUILayout.LabelField(dimension_text, italic_style, options);
            EditorGUILayout.LabelField(breakpoint_text, italic_style, options);
            EditorGUILayout.LabelField(callback_text, italic_style, options);
            EditorGUILayout.EndHorizontal();

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
                if (selected_method_index.ContainsKey(item_to_delete))
                {
                    selected_method_index.Remove(item_to_delete);
                }
            }
            EditorGUI.indentLevel--;

            bool should_add_element
                = renderListButtons();
            if (should_add_element)
            {
                BreakpointEntry new_entry = new BreakpointEntry();
                if (sb_client.BreakpointEntries == null)
                {
                    Debug.Log("sb_client not started yet (but why?). Starting it now");
                    sb_client.Start();
                }
                sb_client.BreakpointEntries.Add(new_entry);
                setMethodSelection(new_entry, DEFAULT_METHOD_INDEX);
                EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// Side-effects: sets listener_methods and selection_labels,
        /// resets all method selections to 0.
        /// </summary>
        /// <precondition>
        /// not called unless the listening_object has actually changed
        /// </precondition>
        /// <param name="listener_type"></param>
        /// <param name="listening_object"></param>
        /// <returns></returns>
        private List<string> retrieveMethodsAndGenerateGUILabels
            (Type listener_type, MonoBehaviour listening_object)
        {
            List<string> selection_labels = new List<string>();
            if (listening_object == null && listener_type != null)
            {
                throw new ArgumentNullException("listening_object");
            }
            else if (listening_object != null && listener_type == null)
            {
                throw new ArgumentNullException("listener_type");
            }
            else if (listening_object != null && listener_type != null)
            {
                //Debug.Log("Got most derived type as " + listening_object_type.ToString());
                listener_methods
                    = new List<MethodInfo>
                        (listener_type.GetMethods(METHODS_TO_SHOW_FLAGS));
                // Select guarantees the same ordering as the source
                // enumeration, so it's fine to use the index from
                // the selection_labels to index into the
                // listener_methods
                selection_labels
                    = listener_methods
                    .Select(x => x.Name)
                    .ToList();
            }
            
            // avoid desync
            // if you just iterate over the keys,
            // it still counts as a desync if you change the values
            // so you have to pull the keys out into a separate list
            List<BreakpointEntry> breakpoints
                = selected_method_index.Keys.ToList();
            // for any case, make sure that methods are set back to null
            foreach (BreakpointEntry selection in breakpoints)
            {
                Debug.Log("Trying to set the method selection for" + selection);
                BreakpointEntry old_selection = selection;
                setMethodSelection(selection, DEFAULT_METHOD_INDEX);
                Debug.Log("New value of selection: " + selection);
                Debug.Log("Selection is "
                         + (old_selection.Equals(selection) ? "" : "not ")
                         + "the same");
            }
            
            return selection_labels;
        }
        
        private bool renderListButtons ()
        {
            bool add_element_button_pressed = false;
            GUILayoutOption max_button_width_opt = GUILayout.MaxWidth(50.0f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            add_element_button_pressed = GUILayout.Button(plus_text, max_button_width_opt);
            EditorGUILayout.EndHorizontal();
            return add_element_button_pressed;
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
            if (!selected_method_index.ContainsKey(entry))
            {
                if (method_names.Contains(entry.handler_function_name))
                {
                    Debug.Log("Setting breakpoint entry "
                            + "using existing function string: "
                            + entry.handler_function_name);
                    int method_index
                        = method_names.IndexOf(entry.handler_function_name);
                    Debug.Log("Function string "
                            + entry.handler_function_name
                            + " gave function index "
                            + method_index.ToString());
                    setMethodSelection(entry, method_index);
                }
                else
                {
                    Debug.Log("Setting breakpoint entry to default");
                    setMethodSelection(entry, DEFAULT_METHOD_INDEX);
                }
            }
            int new_method_index
                = EditorGUILayout.Popup(selected_method_index[entry], method_names.ToArray());
            setMethodSelection(entry, new_method_index);
            bool element_deleted = GUILayout.Button(minus_text, max_button_width_opt);
            EditorGUILayout.EndHorizontal();
            return element_deleted;
        }

        private ScreenSizeChangeLogic.ScreenBreakpointHandler
            generateMethodCallback(MethodInfo method_info)
        {
            return delegate ()
            {
                method_info.Invoke(listening_object, null);
            };
        }

        /// <summary>
        /// Warning: changes the selected_method_index dict
        /// by adding new values if the entry is not found in the dict
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="method_index"></param>
        /// <returns>
        /// true if succesful, false if the given breakpoint does not have
        /// an entry in the selected_method_index structure
        /// </returns>
        private void setMethodSelection (BreakpointEntry entry, int method_index)
        {
            bool set_handler = false;
            if (!selected_method_index.ContainsKey(entry))
            {
                Debug.Log("Entry: " + entry + " not found in dict - adding");
                selected_method_index.Add(entry, method_index);
                set_handler = true;
            }
            // else if method has changed value
            else if (method_index != selected_method_index[entry])
            {
                Debug.Log("Changing method index");
                selected_method_index[entry] = method_index;
                set_handler = true;
            }

            if (set_handler)
            {
                Debug.Log("Changing entry handler");
                if (listening_object != null
                &&  listener_methods.Count > 0)
                {
                    Debug.Log("Using method index " + method_index + " to set new handler.");
                    var new_handler = generateMethodCallback(listener_methods[method_index]);
                    Debug.Log("new handler generated");
                    entry.handler = new_handler;
                    entry.handler_function_name = selection_labels[method_index];
                    entry.callback_instance = listening_object;
                    Debug.Log("new handler set");
                }
                else
                {
                    entry.handler = null;
                }
            }
        }
    }
}

#endif