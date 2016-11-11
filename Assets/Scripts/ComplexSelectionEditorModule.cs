#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SpaceBattles
{
    public abstract class ComplexSelectionEditorModule <ListEntry>
    {
        // -- Const Fields --
        protected readonly BindingFlags SelectionToShowFlags
                = BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.DeclaredOnly;
        private const int DefaultMethodIndex = 0;

        // -- Fields
        public bool IsInitialised = false;
        public UnityEngine.Object ObjectForMethodsToBeInvokedUpon = null;
        public Type TypeForMethodsToBeInvokedUpon = null;

        protected Type MemberSourceType = null;
        protected List<MemberInfo> Members = null;
        private Dictionary<ListEntry, int>
            SelectedMemberIndex = new Dictionary<ListEntry, int>();

        private EntryUpdateHandler OnEntryUpdate = null;
        private GUIContent PlusText = new GUIContent("+");
        private GUILayoutOption MaxButtonWidthOpt = GUILayout.MaxWidth(50.0f);

        protected bool ClassHasNoRelevantMembersToShow = false;

        // -- Delegates --
        public delegate void
            EntryUpdateHandler(
                ListEntry entry,
                MemberInfo handlerInfo,
                string newHandlerFunctionName
            );

        // -- Properties -- 
        public List<string> SelectionNames
        {
            protected set;
            get;
        }

        // -- Methods --
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name=""></typeparam>
        /// <param name="methodListHost"></param>
        /// <param name="objectForMethodsToBeInvokedUpon"></param>
        /// <param name="entryUpdateHandler">
        /// newHandlerFunctionName can be null, in which case
        /// the handler function should be set to null
        /// </param>
        public void
        Initialise
        (EntryUpdateHandler entryUpdateHandler)
        {
            OnEntryUpdate = entryUpdateHandler;

            IsInitialised = true;
        }

        public void
        ProvideObjectForMethodsToBeInvokedUpon
        (UnityEngine.Object newObject)
        {
            ObjectForMethodsToBeInvokedUpon = newObject;
            if (newObject != null)
            {
                Type OldType = MemberSourceType;
                MemberSourceType = ObjectForMethodsToBeInvokedUpon.GetType();
                if (OldType != MemberSourceType)
                {
                    GenerateSelectionLabels();
                    RevertMethodSelectionsToDefault();
                }
            }
            else
            {
                MemberSourceType = null;
            }
        }

        public void
        ProvideTypeForMethodsToBeInvokedUpon
        (Type newType)
        {
            TypeForMethodsToBeInvokedUpon = newType;
            if (newType != null
            &&  newType != MemberSourceType)
            {
                MemberSourceType = newType;
                GenerateSelectionLabels();
                RevertMethodSelectionsToDefault();
            }
            else
            {
                MemberSourceType = newType;
            }
        }


        /// <summary>
        /// Renders the after-list buttons,
        /// in this case just the single 'add element' button
        /// </summary>
        /// <returns>
        /// Whether or not a new element needs to be added to the list
        /// </returns>
        public bool RenderListButtons()
        {
            bool AddElementButtonPressed = false;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            AddElementButtonPressed = GUILayout.Button(PlusText, MaxButtonWidthOpt);
            EditorGUILayout.EndHorizontal();
            return AddElementButtonPressed;
        }

        /// <summary>
        /// Side-effects: if SelectedMethodIndex does not contain an entry
        ///               for the given argument "entry",
        ///               then one will attempt to be created
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="SelectionLabels"></param>
        /// <param name="max_button_width_opt"></param>
        /// <returns></returns>
        public int
        RetrieveEntrysMemberSelection
        (ListEntry entry, string previousSelectionName)
        {
            if (SelectionNames == null
            || (SelectionNames.Count < 1 && !ClassHasNoRelevantMembersToShow))
            {
                //Debug.Log("Recreating selection labels");
                GenerateSelectionLabels();
            }
            // this happens when we are rendering a prefab element
            // i.e. the list has elements already but the viewer isn't set up
            if (!SelectedMemberIndex.ContainsKey(entry))
            {
                //Debug.Log("Could not find "
                //         + entry.ToString()
                //         + " in the SelectedMethodIndex");
                //Debug.Log("Attempting to use previousSelectionName "
                //        + previousSelectionName
                //        + " to rebuild the handler.");
                //Debug.Log("SelectionLabels: "
                //        + PrintIterableStrings(SelectionLabels));

                if (previousSelectionName != null
                && SelectionNames.Contains(previousSelectionName))
                {
                    //Debug.Log("Setting breakpoint entry "
                    //        + "using existing function string: "
                    //        + previousSelectionName);
                    int method_index
                        = SelectionNames.IndexOf(previousSelectionName);
                    //Debug.Log("Function string "
                    //        + previousSelectionName
                    //        + " gave function index "
                    //        + method_index.ToString());
                    SetMemberSelection(entry, method_index);
                }
                else
                {
                    //Debug.Log("Setting breakpoint entry to default");
                    SetMemberSelection(entry, DefaultMethodIndex);
                }
            }
            return SelectedMemberIndex[entry];
        }

        /// <summary>
        /// Warning: changes the selected_method_index dict
        /// by adding new values if the entry is not found in the dict
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="methodIndex"></param>
        public void SetMemberSelection(ListEntry entry, int methodIndex)
        {
            bool SetHandler = false;
            if (!SelectedMemberIndex.ContainsKey(entry))
            {
                //Debug.Log("Entry: " + entry + " not found in dict - adding");
                SelectedMemberIndex.Add(entry, methodIndex);
                SetHandler = true;
            }
            // else if method has changed value
            else if (methodIndex != SelectedMemberIndex[entry])
            {
                //Debug.Log("Changing method index");
                SelectedMemberIndex[entry] = methodIndex;
                SetHandler = true;
            }

            if (SetHandler)
            {
                MyContract.RequireField(
                   IsInitialised,
                   "has been initialised",
                   "Module initialisation state"
                );
                //Debug.Log("Changing entry handler");
                if ((   ObjectForMethodsToBeInvokedUpon != null
                     || TypeForMethodsToBeInvokedUpon   != null)
                && Members.Count > 0)
                {
                    //Debug.Log("Using method index " + methodIndex + " to set new handler.");
                    OnEntryUpdate(entry, Members[methodIndex], SelectionNames[methodIndex]);
                    //Debug.Log("new handler set?");
                }
                else
                {
                    OnEntryUpdate(entry, null, null);
                }
            }
        }

        public void RemoveEntry(ListEntry itemToDelete)
        {
            if (SelectedMemberIndex.ContainsKey(itemToDelete))
            {
                SelectedMemberIndex.Remove(itemToDelete);
            }
        }


        /// <summary>
        /// Side-effects: sets Methods and SelectionLabels,
        /// resets all method selections to 0.
        /// </summary>
        /// <precondition>
        /// Not called unless the ObjectForMethodsToBeInvokedUpon
        /// has actually changed
        /// </precondition>
        /// <param name="target"></param>
        /// <returns></returns>
        protected abstract void GenerateSelectionLabels();

        private void RevertMethodSelectionsToDefault()
        {
            // avoid desync
            // if you just iterate over the keys,
            // it still counts as a desync if you change the values
            // so you have to pull the keys out into a separate list
            List<ListEntry> Entries
                = SelectedMemberIndex.Keys.ToList();
            // for any case, make sure that methods are set back to null
            foreach (ListEntry Entry in Entries)
            {
                //Debug.Log("Trying to set the method selection for" + Entry);
                ListEntry old_selection = Entry;
                SetMemberSelection(Entry, DefaultMethodIndex);
                //Debug.Log("New value of selection: " + Entry);
                //Debug.Log("Selection is "
                //         + (old_selection.Equals(Entry) ? "" : "not ")
                //         + "the same");
            }
        }

        private string PrintIterableStrings(IEnumerable<string> strings)
        {
            string returnstring = "(";
            foreach (string entry in strings)
            {
                returnstring += "[";
                returnstring += entry;
                returnstring += "],";
            }
            returnstring += ")";
            return returnstring;
        }
    }
}

#endif