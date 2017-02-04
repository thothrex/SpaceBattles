using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBattles
{
    [Serializable]
    public class ExplicitLayoutGroup : MonoBehaviour
    {
        // If the names of these fields change,
        // also change the strings used to pull these fields
        // (via reflection)
        // in ExplicitLayoutGroupDrawer
        public LayoutType LayoutType;
        [SerializeField]
        public List<ExplicitLayoutElement> LayoutElements;

        public void applyLayout ()
        {
            foreach (ExplicitLayoutElement element in LayoutElements)
            {
                GameObject target = element.target;
                RectTransform target_transform = target.GetComponent<RectTransform>();
                if (element.ChangeVisibility)
                {
                    target.SetActive(element.visible);
                }
                if (element.do_anchor_max_adjustment)
                {
                    target_transform.anchorMax = element.NewAnchorMax;
                }
                if (element.DoAnchorMinAdjustment)
                {
                    target_transform.anchorMin = element.NewAnchorMin;
                }
                if (element.do_layout_group_alignment)
                {
                    LayoutGroup lg = target.GetComponent<LayoutGroup>();
                    lg.childAlignment = element.layout_group_align;
                }
            }
        }
    }
}

