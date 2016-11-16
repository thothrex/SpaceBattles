using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SpaceBattles
{
    public class ScreenFader : MonoBehaviour
    {
        public Image FadeImg;
        public float FadeTotalDuration = 1.5f;

        private readonly float AcceptableAlphaDifference = 0.0001f;
        private Color TargetColour;
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
                if (FadeCurrentElapsedTime > FadeTotalDuration)
                {
                    FadeImg.color = TargetColour;
                    fading = false;
                }
                else if (TargetColour != null
                && FadeImg != null
                && (Math.Abs(FadeImg.color.a - TargetColour.a)
                        > AcceptableAlphaDifference))
                {
                    // Do fade
                    Color FadeImgColor = FadeImg.color;
                    FadeCurrentElapsedTime += Time.deltaTime;
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
            FadeImg.rectTransform.localScale
                = new Vector2(ScreenSize.width, ScreenSize.height);
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
            TargetColour = targetColour;
            InitialAlpha = FadeImg.color.a;
            AlphaDifference = targetColour.a - InitialAlpha;
            FadeCurrentElapsedTime = 0f;
            fading = true;
        }
    }
}
