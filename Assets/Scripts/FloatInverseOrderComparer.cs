using System;
using System.Collections.Generic;

namespace SpaceBattles
{
    /// <summary>
    /// We allow duplicates by making elements which have the same value
    /// be treated as reflexively greater than one another.
    /// This obviously breaks the ordering assumption about the operator
    /// but hopefully this isn't a problem? 😅
    /// </summary>
    public class FloatInverseOrderAllowDuplicatesComparer : IComparer<float>
    {
        public int Compare(float x, float y)
        {
            int normal_result = y.CompareTo(x);
            if (normal_result != 0) { return normal_result; }
            else                    { return 1; }
        }
    }
}

