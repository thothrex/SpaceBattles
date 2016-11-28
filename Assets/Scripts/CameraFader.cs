using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

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
        private bool Fading = false;
        private Action FadeCompleteCallback = null;

        public void Awake()
        {
            Rect ScreenSizeRect = new Rect(0, 0, Screen.width, Screen.height);
            OnScreenSizeChange(ScreenSizeRect);
        }

        public void Update ()
        {
            if (Fading)
            {
                FadeCurrentElapsedTime += Time.deltaTime;
                if (FadeCurrentElapsedTime > FadeTotalDuration)
                {
                    FadeImg.color = TargetColor;
                    Fading = false;
                    //Debug.Log("Camera Fader: Ending fade");
                    if (FadeCompleteCallback != null)
                    {
                        //Debug.Log("Camera Fader: triggering callback");
                        FadeCompleteCallback();
                        FadeCompleteCallback = null;
                    }
                    else
                    {
                        //Debug.Log("Camera Fader: No callback");
                    }
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

        public void FadeToClear(Action fadeCompleteCallback)
        {
            StartFade(Color.clear, fadeCompleteCallback);
        }

        public void FadeToBlack()
        {
            StartFade(Color.black);
        }

        public void FadeToBlack(Action fadeCompleteCallback)
        {
            StartFade(Color.black, fadeCompleteCallback);
        }

        private void StartFade (Color targetColour)
        {
            //Debug.Log("Starting Fade");
            TargetColor = targetColour;
            InitialAlpha = FadeImg.color.a;
            AlphaDifference = targetColour.a - InitialAlpha;
            FadeCurrentElapsedTime = 0f;
            Fading = true;
        }

        private void
        StartFade
            (Color targetColour,
             Action fadeCompleteCallback)
        {
            if (Fading)
            {
                Debug.LogWarning("Trying to start a fade while already fading");
            }
            FadeCompleteCallback = fadeCompleteCallback;
            StartFade(targetColour);
        }
    }
}
