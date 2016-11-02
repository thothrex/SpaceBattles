using System;
using UnityEngine;

namespace SpaceBattles
{
    public class OrreryUIManager : MonoBehaviour
    {
        // -- Fields --
        public DateTimePicker DateTimePicker;

        // -- Delegates --
        public delegate void ExplicitDateTimeSetHandler (DateTime newTime);

        // -- Events --
        public event ExplicitDateTimeSetHandler DateTimeSet;

        public void BroadcastNewDateTime ()
        {
            DateTimeSet(DateTimePicker.CurrentStoredValue);
        }
    }
}