using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBattles
{
    public class RespawnUIManager : MonoBehaviour
    {
        public readonly string TemporalUnit = "seconds";

        public Text KillerIdentifierDisplay;
        public Text RespawnTimeDisplay;

        private float ElapsedTime = 0;
        private float TimerDuration = 0;
        private bool TimerRunning = false;
        private int IntegerTimeRemaining = 0;

        public void Update ()
        {
            if (TimerRunning)
            {
                ElapsedTime += Time.deltaTime;
                if (ElapsedTime > TimerDuration)
                {
                    TimerRunning = false;
                }
                else
                {
                    DisplayTime(TimerDuration - ElapsedTime);
                }
            }
        }

        public void StartTimer (float timerDuration)
        {
            TimerDuration = timerDuration;
            ElapsedTime = 0;
            TimerRunning = true;
        }

        public void SetKiller (PlayerIdentifier killer)
        {
            KillerIdentifierDisplay.text = killer.ToString();
        }

        private void DisplayTime (float timeRemaining)
        {
            int CurTimeRemaining = (int)Math.Floor(timeRemaining);
            if (CurTimeRemaining != IntegerTimeRemaining)
            {
                IntegerTimeRemaining = CurTimeRemaining;
                RespawnTimeDisplay.text
                    = CurTimeRemaining
                    + " "
                    + TemporalUnit;
            }
        }
    }
}

