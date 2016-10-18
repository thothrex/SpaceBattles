using System;
using UnityEngine;
using UnityEditor;

namespace SpaceBattles
{
    [CustomPropertyDrawer(typeof(ExplicitLayoutElement))]
    public class ExplicitLayoutElementDrawer : PropertyDrawer
    {
        private const float NUM_PROPERTIES = 5.0f;
        private const float PROPERTY_HEIGHT = 20.0f;
        private const float FULL_HEIGHT
            = (NUM_PROPERTIES + 1.0f) * PROPERTY_HEIGHT; // +1 for the foldout
        private const float ARROW_BUTTON_WIDTH = 20.0f;
        private const float TICKBOX_WIDTH = 20.0f;

        private GUIContent name_label       = new GUIContent("Element Name");
        private GUIContent target_label     = new GUIContent("Element Game Object");
        private GUIContent anchor_max_label = new GUIContent("Anchor Max");
        private GUIContent visible_label    = new GUIContent("Element Visible");
        private GUIContent align_label      = new GUIContent("Layout Group Alignment");

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                return FULL_HEIGHT;
            }
            else
            {
                return base.GetPropertyHeight(property, label);
            }
        }

        // Draw the property inside the given rect
        override
        public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty name = property.FindPropertyRelative("name");
            string element_name = name.stringValue;
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            
            if (property.isExpanded)
            {
                //position.height = FULL_HEIGHT;
                
                SerializedProperty target
                    = property.FindPropertyRelative("target");
                SerializedProperty do_anchor_max_adjustment
                    = property.FindPropertyRelative("do_anchor_max_adjustment");
                SerializedProperty anchor_max
                    = property.FindPropertyRelative("NewAnchorMax");
                SerializedProperty visible
                    = property.FindPropertyRelative("visible");
                SerializedProperty do_align
                    = property.FindPropertyRelative("do_layout_group_alignment");
                SerializedProperty alignment
                    = property.FindPropertyRelative("layout_group_align");
                
                // Calculate rects
                Rect foldout_rect
                    = generate_property_rect(position, 0);
                Rect name_rect
                    = generate_property_rect(position, 1);
                Rect target_rect
                    = generate_property_rect(position, 2);
                Rect visible_rect
                    = generate_property_rect(position, 3);
                Rect full_anchor_max_rect
                    = generate_property_rect(position, 4);
                Rect full_align_rect
                    = generate_property_rect(position, 5);

                Rect do_align_rect
                    = new Rect(full_align_rect.x, full_align_rect.y,
                              TICKBOX_WIDTH, full_align_rect.height);
                Rect alignment_rect
                    = new Rect(full_align_rect.x + TICKBOX_WIDTH, full_align_rect.y,
                               full_align_rect.width - TICKBOX_WIDTH, full_align_rect.height);

                Rect do_anchor_max_rect
                    = new Rect(full_anchor_max_rect.x, full_anchor_max_rect.y,
                              TICKBOX_WIDTH, full_anchor_max_rect.height);
                Rect anchor_max_rect
                    = new Rect(full_anchor_max_rect.x + TICKBOX_WIDTH, full_anchor_max_rect.y,
                               full_anchor_max_rect.width - TICKBOX_WIDTH, full_anchor_max_rect.height);

                //EditorGUI.BeginProperty(position, GUIContent.none, property);
                property.isExpanded
                    = EditorGUI.Foldout(foldout_rect, property.isExpanded, GUIContent.none);
                name.stringValue = EditorGUI.TextField(
                    name_rect, name_label, element_name
                );
                EditorGUI.ObjectField(
                    target_rect, target, target_label
                );
                visible.boolValue = EditorGUI.Toggle(
                    visible_rect, visible_label, visible.boolValue
                );

                do_anchor_max_adjustment.boolValue = EditorGUI.Toggle(
                    do_anchor_max_rect,
                    GUIContent.none,
                    do_anchor_max_adjustment.boolValue
                );
                EditorGUI.BeginDisabledGroup(!do_anchor_max_adjustment.boolValue);
                anchor_max.vector2Value = EditorGUI.Vector2Field(
                    anchor_max_rect,
                    anchor_max_label,
                    anchor_max.vector2Value
                );
                EditorGUI.EndDisabledGroup();

                do_align.boolValue
                    = EditorGUI.Toggle(
                        do_align_rect, GUIContent.none, do_align.boolValue
                      );
                EditorGUI.BeginDisabledGroup(!do_align.boolValue);
                TextAnchor current_align = (TextAnchor)alignment.enumValueIndex;
                current_align
                    = (TextAnchor)EditorGUI.EnumPopup(
                        alignment_rect, align_label, current_align
                      );
                alignment.enumValueIndex = (int)current_align;
                EditorGUI.EndDisabledGroup();

                //EditorGUI.EndProperty();
            }
            else
            {
                //EditorGUI.BeginProperty(position, GUIContent.none, property);
                property.isExpanded
                    = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);
                //EditorGUI.EndProperty();
            }
        }

        private Rect generate_property_rect (Rect position, int ordinal)
        {
            return new Rect(position.x, position.y + (PROPERTY_HEIGHT * ordinal),
                            position.width, PROPERTY_HEIGHT);
        }
    }
}
