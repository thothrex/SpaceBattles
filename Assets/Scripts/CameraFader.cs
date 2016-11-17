using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SpaceBattles
{
    public class CameraFader : MonoBehaviour
    {
        public Image FadeImg;
        public float FadeTotalDuration = 1.5f;

        private readonly float AcceptableAlphaDifference = 0.0001f;
        private Color TargetColor;
        private float FadeCurrentElapsedTime = 0f;
        private float InitialAlpha = 1f;
        private float AlphaDifference = 1f;
        private bool fading = false;

        public void Awake()
        {
            Rect ScreenSizeRect = new Rect(0, 0, Screen.width, Screen.height);
            OnScreenSizeChange(ScreenSizeRect);
        }

        public void Update ()
        {
            if (fading)
            {
                FadeCurrentElapsedTime += Time.deltaTime;
                if (FadeCurrentElapsedTime > FadeTotalDuration)
                {
                    FadeImg.color = TargetColor;
                    fading = false;
                    Debug.Log("Ending fade");
                }
                else if (TargetColor != null
                && FadeImg != null
                && (Math.Abs(FadeImg.color.a - TargetColor.a)
                        > AcceptableAlphaDifference))
                {
                    // Do fade
                    Color FadeImgColor = FadeImg.color;
                    float FadeProgress = FadeCurrentElapsedTime
                                        / FadeTotalDuration;
                    float CurrentAlpha
                        = InitialAlpha
                        + FadeProgress * AlphaDifference;
                    FadeImgColor.a = CurrentAlpha;
                    FadeImg.color = FadeImgColor;
                }
            }
        }

        public void OnScreenSizeChange (Rect ScreenSize)
        {
            // no null-propagating operator in c# 4 :(
            if (FadeImg != null)
            {
                FadeImg
                    .rectTransform
                    .SetSizeWithCurrentAnchors(
                        RectTransform.Axis.Horizontal,
                        ScreenSize.width
                     );

                FadeImg
                    .rectTransform
                    .SetSizeWithCurrentAnchors(
                        RectTransform.Axis.Vertical,
                        ScreenSize.height
                     );
            }
        }

        public void FadeToClear()
        {
            StartFade(Color.clear);
        }

        public void FadeToBlack()
        {
            StartFade(Color.black);
        }

        private void StartFade (Color targetColour)
        {
            Debug.Log("Starting Fade");
            TargetColor = targetColour;
            InitialAlpha = FadeImg.color.a;
            AlphaDifference = targetColour.a - InitialAlpha;
            FadeCurrentElapsedTime = 0f;
            fading = true;
        }
    }
}
