using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace SpaceBattles
{
    public class ScoreUiElementManager : MonoBehaviour, IComparable
    {
        public Text NameDisplay;
        public Text ScoreDisplay;

        private bool Initialised = false;

        public int score
        {
            get; private set;
        }

        private static readonly string ScorePostfixString
            = " Kills";
        
        public void Initialise (String name, int score)
        {
            Initialised = true;
            NameDisplay.text = name;
            UpdateScore(score);
        }

        public void UpdateScore (int newScore)
        {
            MyContract.RequireField(Initialised,
                                    "is true",
                                    "Initialised");
            ScoreDisplay.text = newScore.ToString() + ScorePostfixString;
            this.score = newScore;
        }

        public int CompareTo (object obj)
        {
            if (obj == null) { return 1; }

            ScoreUiElementManager otherManager = obj as ScoreUiElementManager;
            if (otherManager != null)
            {
                return this.score.CompareTo(otherManager.score);
            }
            else
            {
                throw new ArgumentException("Object is not a ScoreUiElementManager");
            }
        }
    }
}
