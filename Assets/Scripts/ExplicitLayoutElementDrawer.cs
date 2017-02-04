#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;

namespace SpaceBattles
{
    [CustomPropertyDrawer(typeof(ExplicitLayoutElement))]
    public class ExplicitLayoutElementDrawer : PropertyDrawer
    {
        private const float NUM_PROPERTIES = 6.0f;
        private const float PROPERTY_HEIGHT = 20.0f;
        private const float FULL_HEIGHT
            = (NUM_PROPERTIES + 1.0f) * PROPERTY_HEIGHT; // +1 for the foldout
        private const float ARROW_BUTTON_WIDTH = 20.0f;
        private const float TICKBOX_WIDTH = 20.0f;

        private GUIContent name_label
            = new GUIContent("Element Name");
        private GUIContent target_label
            = new GUIContent("Element Game Object");
        private GUIContent AnchorMinLabel
            = new GUIContent("Anchor Min");
        private GUIContent anchor_max_label
            = new GUIContent("Anchor Max");
        private GUIContent ResultantVisibilityLabel
            = new GUIContent("Element Visible");
        private GUIContent align_label
            = new GUIContent("Layout Group Alignment");

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
                SerializedProperty DoAnchorMinAdjustment
                    = property.FindPropertyRelative("DoAnchorMinAdjustment");
                SerializedProperty AnchorMin
                    = property.FindPropertyRelative("NewAnchorMin");
                SerializedProperty ChangeVisibility
                    = property.FindPropertyRelative("ChangeVisibility");
                SerializedProperty ResultantVisibility
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
                Rect FullVisibilityRect
                    = generate_property_rect(position, 3);
                Rect FullAnchorMinRect
                    = generate_property_rect(position, 4);
                Rect full_anchor_max_rect
                    = generate_property_rect(position, 5);
                Rect full_align_rect
                    = generate_property_rect(position, 6);

                Rect ChangeVisbilityRect
                    = new Rect(FullVisibilityRect.x, FullVisibilityRect.y,
                              TICKBOX_WIDTH, FullVisibilityRect.height);
                Rect ResultantVisibilityRect
                    = new Rect(FullVisibilityRect.x + TICKBOX_WIDTH, FullVisibilityRect.y,
                               FullVisibilityRect.width - TICKBOX_WIDTH, FullVisibilityRect.height);

                Rect do_align_rect
                    = new Rect(full_align_rect.x, full_align_rect.y,
                              TICKBOX_WIDTH, full_align_rect.height);
                Rect alignment_rect
                    = new Rect(full_align_rect.x + TICKBOX_WIDTH, full_align_rect.y,
                               full_align_rect.width - TICKBOX_WIDTH, full_align_rect.height);

                Rect DoAnchorMinRect
                    = new Rect(FullAnchorMinRect.x, FullAnchorMinRect.y,
                              TICKBOX_WIDTH, FullAnchorMinRect.height);
                Rect AnchorMinRect
                    = new Rect(FullAnchorMinRect.x + TICKBOX_WIDTH, FullAnchorMinRect.y,
                               FullAnchorMinRect.width - TICKBOX_WIDTH, FullAnchorMinRect.height);

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

                ChangeVisibility.boolValue = EditorGUI.Toggle(
                    ChangeVisbilityRect,
                    GUIContent.none,
                    ChangeVisibility.boolValue
                );
                EditorGUI.BeginDisabledGroup(!ChangeVisibility.boolValue);
                ResultantVisibility.boolValue = EditorGUI.Toggle(
                    ResultantVisibilityRect,
                    ResultantVisibilityLabel,
                    ResultantVisibility.boolValue
                );
                EditorGUI.EndDisabledGroup();

                DoAnchorMinAdjustment.boolValue = EditorGUI.Toggle(
                    DoAnchorMinRect,
                    GUIContent.none,
                    DoAnchorMinAdjustment.boolValue
                );
                EditorGUI.BeginDisabledGroup(!DoAnchorMinAdjustment.boolValue);
                AnchorMin.vector2Value = EditorGUI.Vector2Field(
                    AnchorMinRect,
                    AnchorMinLabel,
                    AnchorMin.vector2Value
                );
                EditorGUI.EndDisabledGroup();

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

#endif
