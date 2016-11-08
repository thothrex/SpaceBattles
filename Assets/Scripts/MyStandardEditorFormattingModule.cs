#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SpaceBattles
{
    public class MyStandardEditorUI
    {
        private static GUIContent ScriptLabel = new GUIContent("Script");

        private static GUILayoutOption MinWidthOpt = GUILayout.MaxWidth(100.0f);
        private static GUILayoutOption ExpandWidthOpt = GUILayout.ExpandWidth(true);

        public static void
        RenderScriptDisplay
        (MonoBehaviour script)
        {
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = false;
            EditorGUILayout.PrefixLabel(ScriptLabel);
            // We don't want to change the script from here
            // so don't return anything
            EditorGUILayout.ObjectField(
                MonoScript.FromMonoBehaviour(script),
                typeof(MonoBehaviour),
                true
            );
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        public static void
        RenderPreListHeaders
        (GUIContent listTitle, List<GUIContent> columnLabels)
        {
            GUIStyle italic_style = new GUIStyle(GUI.skin.label);
            italic_style.fontStyle = FontStyle.Italic;

            // pre-list setup
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(listTitle);

            EditorGUI.indentLevel++;
            // key as in the key to the table,
            // as in the name of each of the columns
            Rect KeyRect = EditorGUILayout.BeginHorizontal();
            float MaxWidth = KeyRect.width / columnLabels.Count;
            GUILayoutOption MaxWidthOpt = GUILayout.MaxWidth(MaxWidth);
            GUILayoutOption[] options = {
                MaxWidthOpt, MinWidthOpt, ExpandWidthOpt
            };
            //Debug.Log("max width recorded as " + max_width);
            foreach (GUIContent label in columnLabels)
            {
                EditorGUILayout.LabelField(label, italic_style, options);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}

#endif