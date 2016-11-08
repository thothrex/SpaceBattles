#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CordCircuit = SpaceBattles.EventSwitchboard.CordCircuit;

namespace SpaceBattles
{
    [CustomEditor(typeof(EventSwitchboard))]
    public class EventSwitchboardViewer : Editor
    {
        private GUIContent TriggerLabel = new GUIContent("Object To Trigger");
        private GUIContent EnumLabel = new GUIContent("Source Object Registry Type");
        private GUIContent SourceMonoLabel = new GUIContent("Source Monobehaviour Name");
        private GUIContent SourceEventLabel = new GUIContent("Source Unity Event Name");
        private GUIContent RearJackTypeLabel = new GUIContent("Rear Jack Type");
        private string RearJackNameLabelString = "Rear Jack Name";
        private GUIContent RearJackNameLabel = new GUIContent("Rear Jack Name");
        private GUIContent DeleteButtonLabel = new GUIContent("Delete Entry");
        private string SourceKeyLabelString = "Source Object Registry Key";
        GUILayoutOption MaxButtonWidthOpt = GUILayout.MaxWidth(100.0f);
        private int DefaultMethodIndex = 0;

        private EventSwitchboard Switchboard = null;
        private UnityEngine.MonoBehaviour RearJackObject = null;
        private List<string> MethodSelectionLabels = null;
        private List<string> EventSelectionLabels = null;

        private Stack<CordCircuit> CircuitsToDelete
            = new Stack<CordCircuit>();
        private MethodSelectionEditorModule<CordCircuit> MSEM
            = new MethodSelectionEditorModule<CordCircuit>();
        private EventSelectionEditorModule<CordCircuit> ESEM
            = new EventSelectionEditorModule<CordCircuit>();
        private HashSet<CordCircuit> ExpandedCircuits = new HashSet<CordCircuit>();


        private static Type[] SelectableEnumsArray = { typeof(UIElements) };
        private List<Type> SelectableEnums
            = new List<Type>(SelectableEnumsArray);
        private string[] SelectableEnumNames = null;

        private int SelectedEnumIndex = 0;

        /// <summary>
        /// This is called every time we swap back to this
        /// i.e. this will be re-entered while it's still active
        /// </summary>
        public void OnEnable()
        {
            Debug.Log("EventSwitchboardViewer enabled");
            if (target != null)
            {
                Switchboard = (EventSwitchboard)target;
                RearJackObject = Switchboard.Target;

                if (RearJackObject != null)
                {
                    InitialiseSelectionModules(RearJackObject);
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
            MyStandardEditorUI.RenderScriptDisplay(Switchboard);
            RenderOutputObjectSelector(Switchboard);
            RenderEnumSelector(Switchboard);
            RenderCordCircuitList(Switchboard, MethodSelectionLabels);
        }

        public void
        OnCordCircuitEntryUpdate
        (EventSwitchboard.CordCircuit circuit, MemberInfo handlerInfo, string newHandlerFunctionName)
        {
            circuit.RearJackName = newHandlerFunctionName;
            //Debug.Log("Changing entry " + entry.ToString());
            //Debug.Log("Adding handler function \"" + newHandlerFunctionName + "\"");
        }

        private void
        RenderOutputObjectSelector
        (EventSwitchboard switchboard)
        {
            MonoBehaviour CurrentOutputObject = switchboard.Target;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(TriggerLabel);
            MonoBehaviour NewOutputObject
                = (MonoBehaviour)
                EditorGUILayout
                .ObjectField(CurrentOutputObject, typeof(MonoBehaviour), true);

            // If we are just redrawing this element they can be equal
            // if they are literally exactly the same object
            // then don't reassign
            if (NewOutputObject != CurrentOutputObject)
            {
                // regardless of whether or not the new element is null
                // do the assignment
                // (null is valid - no object)
                switchboard.Target = NewOutputObject;
                InitialiseSelectionModules(NewOutputObject);
                EditorUtility.SetDirty(this);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void InitialiseSelectionModules(UnityEngine.Object newOutputObject)
        {
            if (!MSEM.IsInitialised)
            {
                Debug.Log("Initialising MSEM");
                MSEM.Initialise(OnCordCircuitEntryUpdate);
            }
            if (!ESEM.IsInitialised)
            {
                Debug.Log("Initialising ESEM");
                ESEM.Initialise(OnCordCircuitEntryUpdate);
            }
            MSEM.ProvideObjectForMethodsToBeInvokedUpon(newOutputObject);
            ESEM.ProvideObjectForMethodsToBeInvokedUpon(newOutputObject);
            MethodSelectionLabels = MSEM.SelectionNames;
            EventSelectionLabels = ESEM.SelectionNames;
        }

        private void
        RenderEnumSelector
        (EventSwitchboard switchboard)
        {
            Type CurrentEnum = switchboard.RegistryKeyType;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(EnumLabel);

            if (SelectableEnumNames == null)
            {
                SelectableEnumNames
                    = SelectableEnums
                    .Select(t => t.ToString())
                    .ToArray();
            }
            int OldIndex = SelectedEnumIndex;
            SelectedEnumIndex
                = EditorGUILayout.Popup(
                    SelectedEnumIndex,
                    SelectableEnumNames
                  );

            // If we are just redrawing this element they can be equal
            // if they are literally exactly the same object
            // then don't reassign
            if (switchboard.RegistryKeyType == null
            ||  OldIndex != SelectedEnumIndex)
            {
                // regardless of whether or not the new element is null
                // do the assignment
                // (null is valid - no object)
                switchboard.RegistryKeyType = SelectableEnums[SelectedEnumIndex];
                EditorUtility.SetDirty(this);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void
        RenderCordCircuitList
        (EventSwitchboard switchboard, List<string> method_names)
        {
            if (switchboard.CordCircuits != null)
            {
                for (int CircuitIndex = 0;
                    CircuitIndex < switchboard.CordCircuits.Count;
                    CircuitIndex++)
                {
                    CordCircuit Circuit
                        = switchboard.CordCircuits[CircuitIndex];
                    bool ShouldDeleteElement
                        = RenderCircuitEntry(Circuit, CircuitIndex);
                    if (ShouldDeleteElement)
                    {
                        CircuitsToDelete.Push(Circuit);
                    }
                }
            }
            while (CircuitsToDelete.Count > 0)
            {
                CordCircuit ItemToDelete = CircuitsToDelete.Pop();
                switchboard.CordCircuits.Remove(ItemToDelete);
                RemoveFromSelectionModules(ItemToDelete);
            }
            EditorGUI.indentLevel--;

            // static call to EventSelectionEditorModule<CordCircuit>,
            // but this is shorter
            bool ShouldAddEntry = MSEM.RenderListButtons();
            if (ShouldAddEntry)
            {
                CordCircuit NewEntry = new CordCircuit();
                if (switchboard.CordCircuits == null)
                {
                    Debug.Log("CordCircuits null - not sure what to do");
                    switchboard.CordCircuits
                        = new List<EventSwitchboard.CordCircuit>();
                }
                switchboard.CordCircuits.Add(NewEntry);
                NewEntry.RearJackType = EventSwitchboard.JackType.Method;
                MSEM.SetMemberSelection(NewEntry, DefaultMethodIndex);
                EditorUtility.SetDirty(this);
            }
        }

        private void RemoveFromSelectionModules(CordCircuit itemToDelete)
        {
            switch (itemToDelete.RearJackType)
            {
                case EventSwitchboard.JackType.Event:
                    ESEM.RemoveEntry(itemToDelete); break;
                case EventSwitchboard.JackType.Method:
                    MSEM.RemoveEntry(itemToDelete); break;
                default:
                    throw new UnexpectedEnumValueException
                        <EventSwitchboard.JackType>
                            (itemToDelete.RearJackType);
            }
        }

        private void ToggleSelectionModule(CordCircuit itemToChange)
        {
            // Toggles just the selection modules,
            // after the jack type has been changed
            switch (itemToChange.RearJackType)
            {
                case EventSwitchboard.JackType.Event:
                    MSEM.RemoveEntry(itemToChange);
                    ESEM.SetMemberSelection(itemToChange, DefaultMethodIndex);
                    break;
                case EventSwitchboard.JackType.Method:
                    ESEM.RemoveEntry(itemToChange);
                    MSEM.SetMemberSelection(itemToChange, DefaultMethodIndex);
                    break;
                default:
                    throw new UnexpectedEnumValueException
                        <EventSwitchboard.JackType>
                            (itemToChange.RearJackType);
            }
        }

        private bool RenderCircuitEntry(CordCircuit circuit, int index)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.

            bool ElementDeleted = false;
            bool IsExpanded = ExpandedCircuits.Contains(circuit);
            string CircuitName = circuit.RearJackName == null
                               ? index.ToString()
                               : circuit.RearJackName;

            IsExpanded = EditorGUILayout.Foldout(IsExpanded, CircuitName);
            if (IsExpanded)
            {
                //EditorGUI.BeginProperty(position, GUIContent.none, property);
                if (Switchboard.RegistryKeyType != null)
                {
                    string[] EnumNames
                        = Enum.GetNames(Switchboard.RegistryKeyType);
                    circuit.SourceObjectRegistryKey
                        = EditorGUILayout.Popup(
                            SourceKeyLabelString,
                            circuit.SourceObjectRegistryKey,
                            EnumNames
                          );
                }
                else
                {
                    //SourceKey = EditorGUILayout.IntPopup(SourceKey,);
                }

                string SourceMonoName = circuit.SourceMonoBehaviourTypeName;
                circuit.SourceMonoBehaviourTypeName
                    = EditorGUILayout.TextField(SourceMonoLabel, SourceMonoName);

                string SourceEventName = circuit.SourceEventName;
                circuit.SourceEventName
                    = EditorGUILayout.TextField(
                        SourceEventLabel,
                        SourceEventName
                      );

                EventSwitchboard.JackType OldJackType = circuit.RearJackType;
                circuit.RearJackType
                    = (EventSwitchboard.JackType)
                      EditorGUILayout.EnumPopup(RearJackTypeLabel, OldJackType);
                if (OldJackType != circuit.RearJackType)
                {
                    ToggleSelectionModule(circuit);
                }

                if (circuit.RearJackType == EventSwitchboard.JackType.Method)
                {
                    string[] MethodNames
                        = MSEM.SelectionNames.ToArray();
                    int Selection
                        = MSEM.RetrieveEntrysFunctionSelection(
                            circuit,
                            circuit.RearJackName
                          );
                    int NewSelection
                        = EditorGUILayout.Popup(
                            RearJackNameLabelString,
                            Selection,
                            MethodNames
                          );
                    if (Selection != NewSelection)
                    {
                        MSEM.SetMemberSelection(circuit, NewSelection);
                        circuit.RearJackName = MSEM.SelectionNames[NewSelection];
                    }
                }
                else if (circuit.RearJackType == EventSwitchboard.JackType.Event)
                {
                    string[] EventNames
                        = ESEM.SelectionNames.ToArray();
                    int Selection
                        = ESEM.RetrieveEntrysFunctionSelection(
                            circuit,
                            circuit.RearJackName
                            );
                    int NewSelection
                        = EditorGUILayout.Popup(
                            RearJackNameLabelString,
                            Selection,
                            EventNames
                            );
                    if (Selection != NewSelection)
                    {
                        ESEM.SetMemberSelection(circuit, NewSelection);
                        circuit.RearJackName = ESEM.SelectionNames[NewSelection];
                    }
                }
                else
                {
                    string RearJackName = circuit.RearJackName;
                    circuit.RearJackName
                        = EditorGUILayout.TextField(
                            RearJackNameLabel,
                            RearJackName
                          );
                }
                

                ElementDeleted = GUILayout.Button(DeleteButtonLabel, MaxButtonWidthOpt);


                //do_anchor_max_adjustment.boolValue = EditorGUI.Toggle(
                //    do_anchor_max_rect,
                //    GUIContent.none,
                //    do_anchor_max_adjustment.boolValue
                //);
                //EditorGUI.BeginDisabledGroup(!do_anchor_max_adjustment.boolValue);
                //anchor_max.vector2Value = EditorGUI.Vector2Field(
                //    anchor_max_rect,
                //    anchor_max_label,
                //    anchor_max.vector2Value
                //);
                //EditorGUI.EndDisabledGroup();

                //do_align.boolValue
                //    = EditorGUI.Toggle(
                //        do_align_rect, GUIContent.none, do_align.boolValue
                //      );
                //EditorGUI.BeginDisabledGroup(!do_align.boolValue);
                //TextAnchor current_align = (TextAnchor)alignment.enumValueIndex;
                //current_align
                //    = (TextAnchor)EditorGUI.EnumPopup(
                //        alignment_rect, align_label, current_align
                //      );
                //alignment.enumValueIndex = (int)current_align;
                //EditorGUI.EndDisabledGroup();

                //EditorGUI.EndProperty();
                // Do Changes

            }
            // Do Changes
            if (IsExpanded && !ExpandedCircuits.Contains(circuit))
            {
                ExpandedCircuits.Add(circuit);
            }
            else if (!IsExpanded && ExpandedCircuits.Contains(circuit))
            {
                ExpandedCircuits.Remove(circuit);
            }

            return ElementDeleted;
        }

        //EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);
    }
}

#endif
