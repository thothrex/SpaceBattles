#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CordCircuit = SpaceBattles.EventSwitchboard.CordCircuit;
using UnityEditorInternal;

namespace SpaceBattles
{
    [CustomEditor(typeof(EventSwitchboard))]
    public class EventSwitchboardViewer : Editor
    {
        private GUIContent TriggerLabel = new GUIContent("Object To Trigger");
        private GUIContent EnumLabel = new GUIContent("Source Object Registry Type");
        private GUIContent SourceMonoLabel = new GUIContent("Source Monobehaviour Name");
        private string SourceEventLabelString = "Source Unity Event Name";
        private GUIContent SourceEventLabel = new GUIContent("Source Unity Event Name");
        private GUIContent RearJackTypeLabel = new GUIContent("Rear Jack Type");
        private string RearJackNameLabelString = "Rear Jack Name";
        private GUIContent RearJackNameLabel = new GUIContent("Rear Jack Name");
        private GUIContent DeleteButtonLabel = new GUIContent("Delete Entry");
        private string SourceKeyLabelString = "Source Object Registry Key";
        GUILayoutOption MaxButtonWidthOpt = GUILayout.MaxWidth(100.0f);
        private int DefaultMethodIndex = 0;
        private readonly float PropertyHeight = 20.0f;
        private readonly int NumFieldsToDisplayPerCordCircuit = 4;
        

        private UnityEngine.MonoBehaviour RearJackObject = null;
        private List<string> TargetMethodSelectionLabels = null;
        private List<string> TargetEventSelectionLabels = null;
        private List<string> SourceEventSelectionLabels = null;

        private Stack<CordCircuit> CircuitsToDelete
            = new Stack<CordCircuit>();
        private MethodSelectionEditorModule<CordCircuit> TargetMSEM
            = new MethodSelectionEditorModule<CordCircuit>();
        private EventSelectionEditorModule<CordCircuit> TargetESEM
            = new EventSelectionEditorModule<CordCircuit>();
        private Dictionary<int, EventSelectionEditorModule<CordCircuit>>
            UIElementSourceESEMs
                = new Dictionary<int, EventSelectionEditorModule<CordCircuit>>();
        private HashSet<CordCircuit> ExpandedCircuits = new HashSet<CordCircuit>();


        private static Type[] SelectableEnumsArray = { typeof(UIElements) };
        private List<Type> SelectableEnums
            = new List<Type>(SelectableEnumsArray);
        private string[] SelectableEnumNames = null;
        private ReorderableList reorderableList;

        private int SelectedEnumIndex = 0;

        private EventSwitchboard Switchboard
        {
            get
            {
                return target as EventSwitchboard;
            }
        }

        /// <summary>
        /// This is called every time we swap back to this
        /// i.e. this will be re-entered while it's still active
        /// </summary>
        public void OnEnable()
        {
            Debug.Log("EventSwitchboardViewer enabled");
            if (target != null)
            {
                RearJackObject = Switchboard.Target;
                
                if (RearJackObject != null)
                {
                    InitialiseTargetSelectionModules(RearJackObject);
                }
            }

            if (reorderableList == null)
            {
                reorderableList
                    = new ReorderableList(
                        Switchboard.CordCircuits,
                        typeof(CordCircuit),
                        true, true, true, true
                      );
                reorderableList.serializedProperty
                    = serializedObject.FindProperty("CordCircuits");
            }

            reorderableList.drawHeaderCallback += DrawCordCircuitListHeader;
            reorderableList.drawElementCallback += ReorderableListRenderCircuitEntry;
            reorderableList.onAddCallback += ReorderableListAddCordCircuit;
            reorderableList.onRemoveCallback += ReorderableListDeleteCircuit;
            reorderableList.elementHeightCallback += ReorderableListCalculateElementHeight;
        }

        private void OnDisable()
        {
            // Make sure we don't get memory leaks etc.
            reorderableList.drawHeaderCallback -= DrawCordCircuitListHeader;
            reorderableList.drawElementCallback -= ReorderableListRenderCircuitEntry;
            reorderableList.onAddCallback -= ReorderableListAddCordCircuit;
            reorderableList.onRemoveCallback -= ReorderableListDeleteCircuit;
            reorderableList.elementHeightCallback -= ReorderableListCalculateElementHeight;
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
            RenderRegistryKeyTypeSelector(Switchboard);
            //RenderCordCircuitList(Switchboard, TargetMethodSelectionLabels);
            Debug.Assert(reorderableList != null);
            reorderableList.DoLayoutList();
        }

        public void
        OnCordCircuitTargetEntryUpdate
        (EventSwitchboard.CordCircuit circuit, MemberInfo handlerInfo, string newHandlerFunctionName)
        {
            circuit.RearJackName = newHandlerFunctionName;
            //Debug.Log("Changing entry " + entry.ToString());
            //Debug.Log("Adding handler function \"" + newHandlerFunctionName + "\"");
        }

        public void
        OnCordCircuitSourceEntryUpdate
        (EventSwitchboard.CordCircuit circuit, MemberInfo handlerInfo, string newHandlerFunctionName)
        {
            circuit.SourceUnityEventName = newHandlerFunctionName;
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
                InitialiseTargetSelectionModules(NewOutputObject);
                EditorUtility.SetDirty(this);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void InitialiseTargetSelectionModules(UnityEngine.Object newOutputObject)
        {
            UpdateTargetSelectionModule(newOutputObject, TargetESEM);
            UpdateTargetSelectionModule(newOutputObject, TargetMSEM);
            TargetMethodSelectionLabels = TargetMSEM.SelectionNames;
            TargetEventSelectionLabels = TargetESEM.SelectionNames;
        }

        private void
        UpdateTargetSelectionModule
            (UnityEngine.Object newObject,
             ComplexSelectionEditorModule<CordCircuit> SelectionModule)
        {
            if (!SelectionModule.IsInitialised)
            {
                SelectionModule.Initialise(OnCordCircuitTargetEntryUpdate);
            }
            SelectionModule.ProvideObjectForMethodsToBeInvokedUpon(newObject);
        }

        private void
        UpdateSourceSelectionModule
            (Type newType,
             ComplexSelectionEditorModule<CordCircuit> SelectionModule)
        {
            if (!SelectionModule.IsInitialised)
            {
                SelectionModule.Initialise(OnCordCircuitSourceEntryUpdate);
            }
            SelectionModule.ProvideTypeForMethodsToBeInvokedUpon(newType);
        }

        private void
        RenderRegistryKeyTypeSelector
        (EventSwitchboard switchboard)
        {
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
                Debug.Log("Setting registry key type to " + SelectableEnums[SelectedEnumIndex]);
                EditorUtility.SetDirty(this);
            }

            EditorGUILayout.EndHorizontal();
        }

        //private void
        //RenderCordCircuitList
        //(EventSwitchboard switchboard, List<string> method_names)
        //{
        //    if (switchboard.CordCircuits != null)
        //    {
        //        for (int CircuitIndex = 0;
        //            CircuitIndex < switchboard.CordCircuits.Count;
        //            CircuitIndex++)
        //        {
        //            CordCircuit Circuit
        //                = switchboard.CordCircuits[CircuitIndex];
        //            bool ShouldDeleteElement
        //                = RenderCircuitEntry(Circuit, CircuitIndex);
        //            if (ShouldDeleteElement)
        //            {
        //                CircuitsToDelete.Push(Circuit);
        //            }
        //        }
        //    }
        //    while (CircuitsToDelete.Count > 0)
        //    {
        //        CordCircuit ItemToDelete = CircuitsToDelete.Pop();
        //        DeleteCircuit(switchboard, ItemToDelete);
        //    }
        //    EditorGUI.indentLevel--;

        //    // static call to EventSelectionEditorModule<CordCircuit>,
        //    // but this is shorter
        //    bool ShouldAddEntry = TargetMSEM.RenderListButtons();
        //    if (ShouldAddEntry)
        //    {
        //        AddCordCircuit(switchboard);
        //    }
        //}

        private void DeleteCircuit(EventSwitchboard switchboard, CordCircuit ItemToDelete)
        {
            switchboard.CordCircuits.Remove(ItemToDelete);
            RemoveFromSelectionModules(ItemToDelete);
            EditorUtility.SetDirty(target);
        }

        private void AddCordCircuit(EventSwitchboard switchboard)
        {
            CordCircuit NewEntry = new CordCircuit();
            if (switchboard.CordCircuits == null)
            {
                Debug.Log("CordCircuits null - creating list");
                switchboard.CordCircuits
                    = new List<EventSwitchboard.CordCircuit>();
            }
            switchboard.CordCircuits.Add(NewEntry);
            NewEntry.RearJackType = EventSwitchboard.JackType.Method;
            //TargetMSEM.SetMemberSelection(NewEntry, DefaultMethodIndex);
            EditorUtility.SetDirty(target);
        }

        private void
        RemoveFromSelectionModules
            (CordCircuit itemToDelete)
        {
            var SourceESEM
                = RetrieveSourceESEM(itemToDelete);
            SourceESEM.RemoveEntry(itemToDelete);
            switch (itemToDelete.RearJackType)
            {
                case EventSwitchboard.JackType.Event:
                    TargetESEM.RemoveEntry(itemToDelete); break;
                case EventSwitchboard.JackType.Method:
                    TargetMSEM.RemoveEntry(itemToDelete); break;
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
                    TargetMSEM.RemoveEntry(itemToChange);
                    TargetESEM.SetMemberSelection(itemToChange, DefaultMethodIndex);
                    break;
                case EventSwitchboard.JackType.Method:
                    TargetESEM.RemoveEntry(itemToChange);
                    TargetMSEM.SetMemberSelection(itemToChange, DefaultMethodIndex);
                    break;
                default:
                    throw new UnexpectedEnumValueException
                        <EventSwitchboard.JackType>
                            (itemToChange.RearJackType);
            }
        }

        private void
        EnsureSourceEventSelectionIsUpdated
            (CordCircuit circuit,
             EventSelectionEditorModule<CordCircuit> SourceESEM)
        {
            if (Switchboard.RegistryKeyType == typeof(UIElements)
                && ((UIElements)circuit.SourceObjectRegistryKey)
                        .ManagerClass() != null)
            {
                UIElements CircuitElement
                    = (UIElements)circuit.SourceObjectRegistryKey;

                circuit.SourceMonoBehaviourTypeName
                    = CircuitElement.ManagerClass().ToString();

                UpdateSourceSelectionModule(CircuitElement.ManagerClass(),
                                            SourceESEM);

                //Debug.Log("Updated SourceESEM using "
                //        + CircuitElement.ManagerClass().ToString());
            }
            else
            {
                string SourceMonoName = circuit.SourceMonoBehaviourTypeName;
                circuit.SourceMonoBehaviourTypeName
                    = EditorGUILayout.TextField(SourceMonoLabel, SourceMonoName);

                Type SourceMonoType = SourceMonoName == null
                                    ? null
                                    : Type.GetType(SourceMonoName);
                UpdateSourceSelectionModule(SourceMonoType, SourceESEM);

                //Debug.Log("Updated SourceESEM using "
                //        + SourceMonoName);
            }
            //SourceEventSelectionLabels = SourceESEM.SelectionNames;
        }

        private string
        RenderComplexSelectionAndGenerateNewSelectionName
            (CordCircuit circuit,
             ComplexSelectionEditorModule<CordCircuit> selectionModule,
             string previousSelectionName,
             string labelString,
             Rect position)
        {
            // RetrieveEntrysMemberSelection rebuilds SelectionNames
            // as a side-effect, so must be done first
            int Selection
                = selectionModule.RetrieveEntrysMemberSelection(
                    circuit,
                    previousSelectionName
                  );
            

            string[] MemberNames = selectionModule.SelectionNames.ToArray();
            int NewSelection
                = EditorGUI.Popup(
                    position,
                    labelString,
                    Selection,
                    MemberNames
                  );
            if (Selection != NewSelection)
            {
                //Debug.Log("Selection changed to " + NewSelection
                //        + " from " + Selection); 
                selectionModule.SetMemberSelection(circuit, NewSelection);
            }

            if (selectionModule.SelectionNames != null
            &&  selectionModule.SelectionNames.Count > 0)
            {
                return selectionModule.SelectionNames[NewSelection];
            }
            else
            {
                return null;
            }
        }

        private void DrawCordCircuitListHeader(Rect rect)
        {
            GUI.Label(rect, "Cord Circuits");
        }

        private void ReorderableListDeleteCircuit(ReorderableList list)
        {
            DeleteCircuit(Switchboard, Switchboard.CordCircuits[list.index]);
        }

        private void ReorderableListAddCordCircuit(ReorderableList list)
        {
            AddCordCircuit(Switchboard);
        }

        private void ReorderableListRenderCircuitEntry(Rect rect, int index, bool isActive, bool isFocused)
        {
            CordCircuit circuit = Switchboard.CordCircuits[index];

            Debug.Assert(reorderableList != null);
            Debug.Assert(reorderableList.serializedProperty != null);

            string CircuitName = CreateCircuitLabelString(index, circuit);

            SerializedProperty CircuitEntryProperty
                = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            bool IsExpanded = CircuitEntryProperty.isExpanded;
            if (IsExpanded)
            {
                Rect FoldoutRect            = GeneratePropertyRect(rect, 0);
                Rect RegistryKeyRect        = GeneratePropertyRect(rect, 1);
                Rect SourceUnityEventRect   = GeneratePropertyRect(rect, 2);
                Rect RearJackTypeRect       = GeneratePropertyRect(rect, 3);
                Rect RearJackNameRect       = GeneratePropertyRect(rect, 4);
                rect.min                    = FoldoutRect.min;
                rect.max                    = RearJackNameRect.max;
                //Debug.Log("Calculated total rect: " + rect.ToString());
                //Debug.Log("Calculated individual rects: "
                //    + PrintRects(FoldoutRect,
                //                 RegistryKeyRect,
                //                 SourceUnityEventRect,
                //                 RearJackTypeRect,
                //                 RearJackNameRect));

                IsExpanded = EditorGUI.Foldout(FoldoutRect, IsExpanded, CircuitName);
                MaybeRenderRegistryKeySelector(circuit, RegistryKeyRect);
                RenderSourceEventSelector(circuit, SourceUnityEventRect);
                RenderRearJackTypeSelector(circuit, RearJackTypeRect);
                RenderRearJackNameSelector(circuit, RearJackNameRect);
            }
            else
            {
                IsExpanded = EditorGUI.Foldout(rect, IsExpanded, CircuitName);
            }

            // Do expand change
            CircuitEntryProperty.isExpanded = IsExpanded;
            //bool WasExpanded = ExpandedCircuits.Contains(circuit);
            //if (IsExpanded && !WasExpanded)
            //{
            //    ExpandedCircuits.Add(circuit);
            //}
            //else if (!IsExpanded && WasExpanded)
            //{
            //    ExpandedCircuits.Remove(circuit);
            //}
        }

        private static string CreateCircuitLabelString(int index, CordCircuit circuit)
        {
            string CircuitName = index.ToString();
            if (circuit.SourceUnityEventName != null
            && circuit.RearJackName != null)
            {
                CircuitName = circuit.SourceUnityEventName;
                CircuitName += " => ";
                CircuitName += circuit.RearJackName;
                CircuitName += " (";
                CircuitName += circuit.RearJackType;
                CircuitName += ")";
            }

            return CircuitName;
        }

        private void RenderRearJackTypeSelector(CordCircuit circuit, Rect RearJackTypeRect)
        {
            EventSwitchboard.JackType OldJackType = circuit.RearJackType;
            circuit.RearJackType
                = (EventSwitchboard.JackType)
                  EditorGUI.EnumPopup(RearJackTypeRect, RearJackTypeLabel, OldJackType);
            if (OldJackType != circuit.RearJackType)
            {
                ToggleSelectionModule(circuit);
            }
        }

        private void RenderRearJackNameSelector(CordCircuit circuit, Rect RearJackNameRect)
        {
            if (Switchboard.Target != null)
            {
                RenderRearJackNamePopupSelector(circuit, RearJackNameRect);
            }
            else
            {
                RenderRearJackNameBackupSelector(circuit, RearJackNameRect);
            }
        }

        private void RenderSourceEventSelector(CordCircuit circuit, Rect SourceUnityEventRect)
        {
            if (circuit.SourceMonoBehaviourTypeName != null)
            {
                EventSelectionEditorModule<CordCircuit> SourceESEM
                    = RetrieveSourceESEM(circuit);

                EnsureSourceEventSelectionIsUpdated(circuit, SourceESEM);
                circuit.SourceUnityEventName
                    = RenderComplexSelectionAndGenerateNewSelectionName(
                        circuit,
                        SourceESEM,
                        circuit.SourceUnityEventName,
                        SourceEventLabelString,
                        SourceUnityEventRect
                      );
                //Debug.Log("SourceMonoBehaviourTypeName was not null");
            }
            else
            {
                string SourceEventName = circuit.SourceUnityEventName;
                circuit.SourceUnityEventName
                    = EditorGUI.TextField(
                        SourceUnityEventRect,
                        SourceEventLabel,
                        SourceEventName
                      );
                //Debug.Log("SourceMonoBehaviourTypeName was null");
            }
        }

        private EventSelectionEditorModule<CordCircuit> RetrieveSourceESEM(CordCircuit circuit)
        {
            if (!UIElementSourceESEMs
                .ContainsKey(circuit.SourceObjectRegistryKey))
            {
                int NewKey = circuit.SourceObjectRegistryKey;
                var NewSourceESEM
                    = new EventSelectionEditorModule<CordCircuit>();
                UIElementSourceESEMs.Add(NewKey, NewSourceESEM);
            }

            return UIElementSourceESEMs[circuit.SourceObjectRegistryKey];
        }

        private void RenderRearJackNamePopupSelector(CordCircuit circuit, Rect RearJackNameRect)
        {
            if (circuit.RearJackType == EventSwitchboard.JackType.Method)
            {
                circuit.RearJackName
                    = RenderComplexSelectionAndGenerateNewSelectionName(
                        circuit,
                        TargetMSEM,
                        circuit.RearJackName,
                        RearJackNameLabelString,
                        RearJackNameRect
                      );
            }
            else if (circuit.RearJackType == EventSwitchboard.JackType.Event)
            {
                circuit.RearJackName
                    = RenderComplexSelectionAndGenerateNewSelectionName(
                        circuit,
                        TargetESEM,
                        circuit.RearJackName,
                        RearJackNameLabelString,
                        RearJackNameRect
                      );
            }
            else
            {
                RenderRearJackNameBackupSelector(circuit, RearJackNameRect);
            }
        }

        private void RenderRearJackNameBackupSelector(CordCircuit circuit, Rect RearJackNameRect)
        {
            string RearJackName = circuit.RearJackName;
            circuit.RearJackName
                = EditorGUI.TextField(
                    RearJackNameRect,
                    RearJackNameLabel,
                    RearJackName
                  );
        }

        private void MaybeRenderRegistryKeySelector(CordCircuit circuit, Rect rect)
        {
            if (Switchboard.RegistryKeyType == null)
            {
                return;
            }

            Type EnumType       = Switchboard.RegistryKeyType;
            int oldKey          = circuit.SourceObjectRegistryKey;
            string[] EnumNames  = Enum.GetNames(EnumType);
            int oldIndex        = ConvertKeyToIndex(oldKey, EnumType);
            int newIndex
                = EditorGUI.Popup(
                    rect,
                    SourceKeyLabelString,
                    oldIndex,
                    EnumNames
                  );

            if (oldIndex != newIndex)
            {
                circuit.SourceObjectRegistryKey
                    = ConvertIndexToKey(newIndex, EnumType);
                var SourceESEM
                    = RetrieveSourceESEM(circuit);
                EnsureSourceEventSelectionIsUpdated(circuit, SourceESEM);
            }
        }

        private static int ConvertIndexToKey(int newIndex, Type enumType)
        {
            string[] EnumNames = Enum.GetNames(enumType);
            string newName = EnumNames[newIndex];
            return (int)Enum.Parse(enumType, newName);
        }

        private int ConvertKeyToIndex(int oldKey, Type enumType)
        {
            // we do it this way
            // (rather than by bit-masking)
            // to support combined values
            // e.g. 5, 7, etc.
            string[] EnumNames = Enum.GetNames(enumType);
            string oldSelectionName = Enum.GetName(enumType, oldKey);
            return Array.IndexOf(EnumNames, oldSelectionName);
        }

        private Rect GeneratePropertyRect(Rect position, int ordinal)
        {
            return new Rect(position.x, position.y + (PropertyHeight * ordinal),
                            position.width, PropertyHeight);
        }

        public float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                return PropertyHeight // +1 for the header;
                     * (NumFieldsToDisplayPerCordCircuit + 1); 
            }
            else
            {
                return PropertyHeight;
            }
        }

        private float ReorderableListCalculateElementHeight(int index)
        {
            SerializedProperty CircuitEntryProperty
                = reorderableList
                .serializedProperty
                .GetArrayElementAtIndex(index);
            return GetPropertyHeight(CircuitEntryProperty, GUIContent.none);
        }

        private string PrintRects (params Rect[] rects)
        {
            return PrintRectsHelper(rects);
        }

        private string PrintRects (IEnumerable<Rect> rects)
        {
            return PrintRectsHelper(rects);
        }

        private string PrintRectsHelper (IEnumerable<Rect> rects)
        {
            string returnstring = "Rects: ";
            foreach (Rect rect in rects)
            {
                returnstring += "\n";
                returnstring += rect.ToString();
            }
            returnstring += "\n";
            return returnstring;
        }
    }
}

#endif
