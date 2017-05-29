using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBattles
{
    public class ScreenRotationEnforcer : MonoBehaviour
    {
        public RectTransform EnforcementTarget;

        private Resolution StartResolution;
        private Resolution PreviousResolution;
        private ScreenOrientation PreviousOrientation;

        public void Awake()
        {
            StartResolution = Screen.currentResolution;
            if (EnforcementTarget == null)
            {
                EnforcementTarget = gameObject.GetComponent<RectTransform>();
            }
            MyContract.RequireFieldNotNull(
                EnforcementTarget, "EnforcementTarget"
            );
            EnforcementTarget.sizeDelta
                = new Vector2(Screen.width, Screen.height);
            EnforcementTarget.rotation
                = Quaternion.Euler(
                    0,
                    0,
                    GenerateTargetScreenRotation(Screen.orientation)
                );
        }

        public void Update()
        {
            Resolution CurrentResolution = Screen.currentResolution;
            ScreenOrientation CurrentOrientation = Screen.orientation;
            bool ResolutionChanged
                = CurrentResolution.height != PreviousResolution.height
                || CurrentResolution.width != PreviousResolution.width;
            bool OrientationChanged = CurrentOrientation != PreviousOrientation;

            if (OrientationChanged)
            {
                //Rect EnforcingRect = EnforcementTarget.rect;
                //Rect TargetRect
                //    = GenerateTargetScreenRect(
                //        StartResolution,
                //        CurrentOrientation
                //    );
                //if (EnforcingRect != TargetRect)
                //{
                    EnforcementTarget.rotation
                        = Quaternion.Euler(
                            0,
                            0,
                            GenerateTargetScreenRotation(CurrentOrientation)
                        );
                //}
            }
            
            PreviousOrientation = CurrentOrientation;
            PreviousResolution = CurrentResolution;
        }

        private static bool IsLandscape (ScreenOrientation orientation)
        {
            return orientation == ScreenOrientation.LandscapeLeft
                || orientation == ScreenOrientation.LandscapeRight
                || orientation == ScreenOrientation.Landscape;
        }

        private static bool IsPortrait (ScreenOrientation orientation)
        {
            return orientation == ScreenOrientation.Portrait
                || orientation == ScreenOrientation.PortraitUpsideDown;
        }

        private Rect
        GenerateTargetScreenRect
            (Resolution startResolution, ScreenOrientation orientation)
        {
            bool WasInitiallyPortrait
                = startResolution.width < startResolution.height;
            bool ShouldFlip = WasInitiallyPortrait != IsPortrait(orientation);

            float TargetWidth
                = ShouldFlip ? startResolution.height : startResolution.width;
            float TargetHeight
                = ShouldFlip ? startResolution.width : startResolution.height;

            return new Rect(0, 0, TargetWidth, TargetHeight);
        }
        
        private float
        GenerateTargetScreenRotation
            (ScreenOrientation orientation)
        {
            switch (orientation)
            {
                case ScreenOrientation.Portrait:
                    return 0;
                case ScreenOrientation.LandscapeLeft:
                    return 90;
                case ScreenOrientation.PortraitUpsideDown:
                    return 180;
                case ScreenOrientation.LandscapeRight:
                    return 270;
                default:
                    Debug.LogWarning(
                        "Unexpected target rotation ("
                        + orientation.ToString()
                        + ") in ScreenRotationEnforcer."
                        + "GenerateTargetScreenRotation"
                    );
                    return 0;
            }

        }
    }
}

