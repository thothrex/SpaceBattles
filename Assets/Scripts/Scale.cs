using System;
using UnityEngine;

namespace SpaceBattles
{
    // Realistically I'm only expecting this to be used for
    // OrbitingBodyBackgroundGameObject,
    // but enum extensionsa require a non-nested class,
    // so I can't make them internal to that class.

    public enum Scale
    {
        Metres, NearestPlanet, SolarSystem, Logarithmic
    }

    public static class ScaleExtensions
    {
        /// <summary>
        /// You multiply all values which are in the nearest planet scale
        /// by this value to get their size in metres.
        /// (Or divide any value in metres by this value
        ///  to get the value in the nearest planet scale).
        /// </summary>
        public static readonly double
            NearestPlanetsMetresMultiplier = 1000.0;
        public static readonly double LogBase = 10.0;

        public static double ConvertMeasurementToMetres (this Scale scale, double measurement)
        {
            switch (scale)
            {
                case Scale.Metres:
                    return measurement;
                case Scale.NearestPlanet:
                    return measurement * NearestPlanetsMetresMultiplier;
                case Scale.SolarSystem:
                    return measurement * OrbitingBodyMathematics.DISTANCE_SCALE_TO_METRES;
                case Scale.Logarithmic:
                    return Math.Pow(LogBase, measurement);
                default:
                    throw new UnexpectedEnumValueException<Scale>(scale);
            }
        }

        public static double ConvertMeasurementFromMetres(this Scale scale, double measurement)
        {
            switch (scale)
            {
                case Scale.Metres:
                    return measurement;
                case Scale.NearestPlanet:
                    return measurement / NearestPlanetsMetresMultiplier;
                case Scale.SolarSystem:
                    return measurement / OrbitingBodyMathematics.DISTANCE_SCALE_TO_METRES;
                case Scale.Logarithmic:
                    return Math.Log(measurement, LogBase);
                default:
                    throw new UnexpectedEnumValueException<Scale>(scale);
            }
        }

        /// <summary>
        /// Converts valueToScale from this enum's scale
        /// to the targetScale's scale.
        /// </summary>
        /// <param name="initialScale">
        /// The scale to change valueToScale FROM
        /// </param>
        /// <param name="targetScale">
        /// The scale to change valueToScale TO
        /// </param>
        /// <param name="valueToScale"></param>
        /// <returns></returns>
        public static double
        ConvertMeasurementTo
            (this Scale initialScale,
             Scale targetScale,
             double valueToScale)
        {
            double MetreValues
                = initialScale.ConvertMeasurementToMetres(valueToScale);
            return targetScale.ConvertMeasurementFromMetres(MetreValues);
        }
    }
}
