#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SpaceBattles
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="ListEntry">
    /// Represents the individual entries of the parent's list
    /// which will require a method field
    /// e.g. CordCircuit, BreakpointEntry
    /// </typeparam>
    public class EventSelectionEditorModule<ListEntry> : ComplexSelectionEditorModule<ListEntry>
    {
        protected override void GenerateSelectionLabels()
        {
            SelectionNames = new List<string>();
            if (ObjectForMethodsToBeInvokedUpon != null)
            {
                MyContract.RequireFieldNotNull(MemberSourceType, "MemberSourceType");
                //Debug.Log("Got most derived type as " + MethodSourceType.ToString());
                Members
                    = new List<MemberInfo>
                        (MemberSourceType
                            .GetEvents(SelectionToShowFlags)
                        );
                ClassHasNoRelevantMembersToShow = Members.Count == 0;

                // Select guarantees the same ordering as the source
                // enumeration, so it's fine to use the index from
                // the selection_labels to index into the
                // listener_methods
                SelectionNames
                    = Members
                    .Select(x => x.Name)
                    .ToList();
            }
            else
            {
                //Debug.Log("Object is null, no SelectionLabels required");
            }
        }
    }
}

#endif
