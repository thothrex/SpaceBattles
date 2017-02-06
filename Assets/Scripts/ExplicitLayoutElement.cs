using System;
using UnityEngine;

namespace SpaceBattles
{
    [Serializable]
    public class ExplicitLayoutElement
    {
        // If the names of these fields change,
        // also change the strings used to pull these fields
        // (via reflection)
        // in ExplicitLayoutElementDrawer
        public string name;
        public bool ChangeVisibility;
        public bool visible;
        public bool do_layout_group_alignment;
        public bool do_anchor_max_adjustment;
        public bool DoAnchorMinAdjustment;
        public GameObject target;
        public Vector2 NewAnchorMax;
        public Vector2 NewAnchorMin;
        public TextAnchor layout_group_align;
    }
}
