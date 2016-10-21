using System;

namespace SpaceBattles
{
    // Realistically I'm only expecting this to be used for
    // OrbitingBodyBackgroundGameObject,
    // but enum extensionsa require a non-nested class,
    // so I can't make them internal to that class.

    public enum Scale
    {
        Metres, NearestPlanet, SolarSystem
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

        public static double MetresMultiplier(this Scale scale)
        {
            switch(scale)
            {
                case Scale.Metres:
                    return 1.0;
                case Scale.NearestPlanet:
                    return NearestPlanetsMetresMultiplier;
                case Scale.SolarSystem:
                    return OrbitingBodyMathematics.DISTANCE_SCALE_TO_METRES;
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
            double ScaleRatio = targetScale.MetresMultiplier()
                               / initialScale.MetresMultiplier();
            return valueToScale * ScaleRatio;
        }
    }
}
