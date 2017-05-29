using UnityEngine;
using UnityEditor;
using System;
using NUnit.Framework;

namespace SpaceBattles
{
    public class OrbitTest
    {
        public String
        differing_results_error_message (String actual,
                                         String expected,
                                         String units)
        {
            return "Got " + actual + " " + units
                 + ", expecting " + expected + " " + units;
        }

        /// <summary>
        /// data from http://nssdc.gsfc.nasa.gov/planetary/factsheet/
        /// </summary>
        /// <param name="expected_orbital_period"></param>
        /// <param name="planet"></param>
        public void
        OrbitalPeriodTest (double expected_orbital_period,
                           OrbitingBodyMathematics planet)
        {
            double orbital_period          = planet.get_orbital_period(); // in seconds
            double orbital_period_in_days  = orbital_period / 60 / 60 / 24;

            double acceptable_error = 0.001;
            double actual_error = Math.Abs(orbital_period_in_days - expected_orbital_period);
            string error_message = differing_results_error_message(
                Convert.ToString(orbital_period_in_days),
                Convert.ToString(expected_orbital_period),
                "days"
            );
            Assert.LessOrEqual(actual_error, acceptable_error, error_message);
        }

        /// <summary>
        /// data generated from http://www.wolframalpha.com
        /// </summary>
        /// <param name="expected_mean_anomaly"></param>
        /// <param name="test_time"></param>
        /// <param name="planet"></param>
        public void 
        MeanAnomalyTest(double expected_mean_anomaly,
                        DateTime test_time,
                        OrbitingBodyMathematics planet)
        {
            double mean_anomaly = planet.mean_anomaly(test_time); // in radians

            double acceptable_error = 0.005;
            double actual_error = Math.Abs(mean_anomaly - expected_mean_anomaly);
            string error_message = differing_results_error_message(
                Convert.ToString(expected_mean_anomaly),
                Convert.ToString(mean_anomaly),
                "radians"
            );
            Assert.LessOrEqual(actual_error, acceptable_error, error_message);
        }

        /// <summary>
        /// Expected value retrieved from e.g. http://m.wolframalpha.com/input/?i=Mercury+eccentric+anomaly&x=0&y=0
        /// </summary>
        /// <param name="expected_eccentric_anomaly"></param>
        /// <param name="test_time"></param>
        /// <param name="planet"></param>
        public void
        EccentricAnomalyTest(double expected_eccentric_anomaly,
                            DateTime test_time,
                            OrbitingBodyMathematics planet)
        {
            double eccentric_anomaly = planet.eccentric_anomaly(test_time, 100); // in radians

            double acceptable_error = 0.005;
            double actual_error = Math.Abs(eccentric_anomaly - expected_eccentric_anomaly);
            string error_message = differing_results_error_message(
                Convert.ToString(expected_eccentric_anomaly),
                Convert.ToString(eccentric_anomaly),
                "radians"
            );
            Assert.LessOrEqual(actual_error, acceptable_error, error_message);
        }

        /// <summary>
        /// long lat and distance data retrieved from http://omniweb.gsfc.nasa.gov/coho/helios/planet.html
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="test_time"></param>
        /// <param name="expected_longitude"></param>
        /// <param name="expected_latitude"></param>
        /// <param name="expected_distance"></param>
        public void
        PositionTest(OrbitingBodyMathematics planet,
                     DateTime test_time,
                     double expected_longitude,
                     double expected_latitude,
                     double expected_distance)
        {
            string expected_string
                = "\nExpected: longitude " + expected_longitude.ToString("F5")
                + " degrees, latitude " + expected_latitude.ToString("F5")
                + " degrees, distance " + expected_distance.ToString("F5");
            Vector3 coordinates = planet.current_location(test_time);
            Vector3 longlatdist = planet.current_longlatdist(test_time);

            Console.Write(coordinates.ToString("E"));
            Console.Write(longlatdist.ToString("F5"));

            double longitude_error = Math.Abs(longlatdist.x - expected_longitude);
            double acceptable_longitude_error = 0.05;
            string longitude_error_message = differing_results_error_message(
                Convert.ToString(longlatdist.x),
                Convert.ToString(expected_longitude),
                "degrees"
            );
            Assert.LessOrEqual(longitude_error, acceptable_longitude_error,
                               "Longitude Error: " + longitude_error_message + expected_string);

            double latitude_error = Math.Abs(longlatdist.y - expected_latitude);
            double acceptable_latitude_error = 0.01;
            string latitude_error_message = differing_results_error_message(
                Convert.ToString(longlatdist.y),
                Convert.ToString(expected_latitude),
                "degrees"
            );
            Assert.LessOrEqual(latitude_error, acceptable_latitude_error,
                               "Latitude Error: " + latitude_error_message + expected_string);

            double distance_error = Math.Abs(longlatdist.z - expected_distance);
            double acceptable_distance_error = 0.00001;
            string distance_error_message = differing_results_error_message(
                Convert.ToString(longlatdist.z),
                Convert.ToString(expected_distance),
                "AU"
            );
            Assert.LessOrEqual(distance_error, acceptable_distance_error,
                               "Distance Error: " + distance_error_message + expected_string);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected_rotation_progress"></param>
        /// <param name="test_time"></param>
        /// <param name="planet"></param>
        public void
        RotationAbsoluteTest(double expected_rotation_progress,
                     DateTime test_time,
                     OrbitingBodyMathematics planet)
        {
            double actual_rotation_progress
                = planet.current_stellar_day_rotation_progress(test_time);

            double acceptable_error = 0.001;
            double actual_error
                = Math.Abs(actual_rotation_progress
                         - expected_rotation_progress);
            string error_message = differing_results_error_message(
                Convert.ToString(actual_rotation_progress),
                Convert.ToString(expected_rotation_progress),
                ""
            );
            Assert.LessOrEqual(actual_error, acceptable_error, error_message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected_rotation_progress"></param>
        /// <param name="test_time"></param>
        /// <param name="planet"></param>
        public void
        RotationRelativeToSunTest(double expected_angle,
                                  DateTime test_time,
                                  OrbitingBodyMathematics planet)
        {
            Vector3 planet_position
                = planet.current_location(test_time);
            Debug.Log("Planet position 3D: " + planet_position.ToString());
            Vector2 planet_position_2d
                = new Vector2(planet_position.x, planet_position.y);

            double rotation_progress
                = planet.current_stellar_day_rotation_progress(test_time);
            Debug.Log("Rotation progress: " + rotation_progress);
            Debug.Log("Anti-clockwise angle = " + (rotation_progress * 360.0));
            float angle_from_up = (float)rotation_progress * 2 * Mathf.PI;
            // angle is anti-clockwise due to earth's/solar system's rotation
            Vector2 rotation
                = new Vector2(Mathf.Sin(angle_from_up), Mathf.Cos(angle_from_up));

            Vector2 planet_normalised = planet_position_2d / planet_position_2d.magnitude;
            Vector2 rotation_normalised = rotation / rotation.magnitude;

            Debug.Log("Planet vector: " + planet_normalised.ToString());
            Debug.Log("Rotation vector: " + rotation_normalised.ToString());

            double actual_angle
                = Vector2.Angle(planet_normalised, rotation_normalised);

            double acceptable_error = 0.1;
            double actual_error
                = Math.Abs(actual_angle
                         - expected_angle);

            string error_message = differing_results_error_message(
                Convert.ToString(actual_angle),
                Convert.ToString(expected_angle),
                "degrees"
            );
            Assert.LessOrEqual(actual_error, acceptable_error, error_message);
        }

        // --------------------------------------

        [Test]
        public void MercuryOrbitalPeriodTest()
        {
            OrbitingBodyMathematics mercury = OrbitingBodyMathematics.generate_mercury();
            OrbitalPeriodTest(87.969, mercury);
        }

        [Test]
        public void MercuryMeanAnomalyTest()
        {
            OrbitingBodyMathematics mercury = OrbitingBodyMathematics.generate_mercury();
            var test_time = new DateTime(2016, 2, 10, 11, 48, 00);
            MeanAnomalyTest(2.34, test_time, mercury);
        }

        [Test]
        public void MercuryEccentricAnomalyTest()
        {
            OrbitingBodyMathematics mercury = OrbitingBodyMathematics.generate_mercury();
            var test_time = new DateTime(2016, 2, 10, 11, 49, 0);
            EccentricAnomalyTest(2.468, test_time, mercury);
        }

        [Test]
        public void MercuryPositionTest()
        {
            OrbitingBodyMathematics mercury = OrbitingBodyMathematics.generate_mercury();
            var test_time = new DateTime(2016, 2, 10);
            PositionTest(mercury, test_time, 224.43, 0.13, 0.451);
        }

        // ------------------------------------------------------------
        [Test]
        public void VenusOrbitalPeriodTest()
        {
            OrbitingBodyMathematics venus = OrbitingBodyMathematics.generate_venus();
            OrbitalPeriodTest(224.701, venus);
        }

        [Test]
        public void VenusMeanAnomalyTest()
        {
            OrbitingBodyMathematics venus = OrbitingBodyMathematics.generate_venus();
            var test_time = new DateTime(2016, 2, 10, 12, 17, 00);
            MeanAnomalyTest(2.045, test_time, venus);
        }

        [Test]
        public void VenusEccentricAnomalyTest()
        {
            OrbitingBodyMathematics venus = OrbitingBodyMathematics.generate_venus();
            var test_time = new DateTime(2016, 2, 10, 12, 18, 00);
            EccentricAnomalyTest(2.051, test_time, venus);
        }

        [Test]
        public void VenusPositionTest()
        {
            OrbitingBodyMathematics venus = OrbitingBodyMathematics.generate_venus();
            var test_time = new DateTime(2016, 2, 10);
            PositionTest(venus, test_time, 249.01, 0.46, 0.726);
        }

        // ------------------------------------------------------------

        [Test]
        public void EarthOrbitalPeriodTest()
        {
            OrbitingBodyMathematics earth = OrbitingBodyMathematics.generate_earth();
            OrbitalPeriodTest(365.25636, earth);
        }

        [Test]
        public void EarthMeanAnomalyTest1()
        {
            OrbitingBodyMathematics earth = OrbitingBodyMathematics.generate_earth();
            var test_time = new DateTime(2016, 2, 9, 14, 49, 00);
            MeanAnomalyTest(0.6441, test_time, earth);
        }

        [Test]
        public void EarthMeanAnomalyTest2()
        {
            OrbitingBodyMathematics earth = OrbitingBodyMathematics.generate_earth();
            var test_time = new DateTime(2016, 2, 10, 9, 54, 00);
            MeanAnomalyTest(0.6577, test_time, earth);
        }

        [Test]
        public void EarthEccentricAnomalyTest()
        {
            OrbitingBodyMathematics earth = OrbitingBodyMathematics.generate_earth();
            var test_time = new DateTime(2016, 2, 10, 13, 25, 00);
            EccentricAnomalyTest(0.6705, test_time, earth);
        }

        [Test]
        public void EarthPositionTest()
        {
            OrbitingBodyMathematics earth = OrbitingBodyMathematics.generate_earth();
            var test_time = new DateTime(2016, 2, 10);
            PositionTest(earth, test_time, 140.65, 0.0, 0.987);
        }

        // ------------------------------------------------------------

        [Test]
        public void MoonOrbitalPeriodTest()
        {
            OrbitingBodyMathematics earth = OrbitingBodyMathematics.generate_earth();
            OrbitingBodyMathematics moon = OrbitingBodyMathematics.generate_moon(earth);
            OrbitalPeriodTest(27.3217, moon);
        }

        [Test]
        public void MoonMeanAnomalyTest()
        {
            OrbitingBodyMathematics earth = OrbitingBodyMathematics.generate_earth();
            OrbitingBodyMathematics moon = OrbitingBodyMathematics.generate_moon(earth);
            var test_time = new DateTime(2016, 2, 23, 10, 28, 00);
            MeanAnomalyTest(2.747, test_time, moon);
        }

        [Test]
        public void MoonEccentricAnomalyTest()
        {
            OrbitingBodyMathematics earth = OrbitingBodyMathematics.generate_earth();
            OrbitingBodyMathematics moon = OrbitingBodyMathematics.generate_moon(earth);
            var test_time = new DateTime(2016, 2, 23, 10, 30, 00);
            EccentricAnomalyTest(2.768, test_time, moon);
        }

        [Test]
        public void MoonPositionTest()
        {
            OrbitingBodyMathematics earth = OrbitingBodyMathematics.generate_earth();
            OrbitingBodyMathematics moon = OrbitingBodyMathematics.generate_moon(earth);
            var test_time = new DateTime(2016, 2, 10);
            // TODO: Get geo-centric coordinates
            PositionTest(earth, test_time, 140.65, 0.0, 0.987);
        }

        // ------------------------------------------------------------

        [Test]
        public void MarsOrbitalPeriodTest()
        {
            OrbitingBodyMathematics mars = OrbitingBodyMathematics.generate_mars();
            OrbitalPeriodTest(686.980, mars);
        }

        [Test]
        public void MarsMeanAnomalyTest()
        {
            OrbitingBodyMathematics mars = OrbitingBodyMathematics.generate_mars();
            var test_time = new DateTime(2016, 2, 10, 12, 32, 00);
            MeanAnomalyTest(3.887, test_time, mars);
        }

        [Test]
        public void MarsEccentricAnomalyTest()
        {
            OrbitingBodyMathematics mars = OrbitingBodyMathematics.generate_mars();
            var test_time = new DateTime(2016, 2, 10, 12, 33, 00);
            EccentricAnomalyTest(3.828, test_time, mars);
        }

        [Test]
        public void MarsPositionTest()
        {
            OrbitingBodyMathematics mars = OrbitingBodyMathematics.generate_mars();
            var test_time = new DateTime(2016, 2, 10);
            PositionTest(mars, test_time, 192.34, 1.13, 1.634);
        }

        // ------------------------------------------------------------

        [Test]
        public void JupiterOrbitalPeriodTest()
        {
            OrbitingBodyMathematics jupiter = OrbitingBodyMathematics.generate_jupiter();
            OrbitalPeriodTest(4332.589, jupiter);
        }

        [Test]
        public void JupiterMeanAnomalyTest()
        {
            OrbitingBodyMathematics jupiter = OrbitingBodyMathematics.generate_jupiter();
            var test_time = new DateTime(2016, 2, 10, 13, 12, 00);
            MeanAnomalyTest(2.599, test_time, jupiter);
        }

        [Test]
        public void JupiterEccentricAnomalyTest()
        {
            OrbitingBodyMathematics jupiter = OrbitingBodyMathematics.generate_jupiter();
            var test_time = new DateTime(2016, 2, 10, 13, 27, 00);
            EccentricAnomalyTest(2.623, test_time, jupiter);
        }

        [Test]
        public void JupiterPositionTest()
        {
            OrbitingBodyMathematics jupiter = OrbitingBodyMathematics.generate_jupiter();
            var test_time = new DateTime(2016, 2, 10);
            PositionTest(jupiter, test_time, 166.22, 1.19, 5.418);
        }

        // ------------------------------------------------------------

        [Test]
        public void SaturnOrbitalPeriodTest()
        {
            OrbitingBodyMathematics saturn = OrbitingBodyMathematics.generate_saturn();
            OrbitalPeriodTest(10759.22, saturn);
        }

        [Test]
        public void SaturnMeanAnomalyTest()
        {
            OrbitingBodyMathematics saturn = OrbitingBodyMathematics.generate_saturn();
            var test_time = new DateTime(2016, 2, 10, 13, 43, 00);
            MeanAnomalyTest(2.685, test_time, saturn);
        }

        [Test]
        public void SaturnEccentricAnomalyTest()
        {
            OrbitingBodyMathematics saturn = OrbitingBodyMathematics.generate_saturn();
            var test_time = new DateTime(2016, 2, 10, 13, 44, 00);
            EccentricAnomalyTest(2.707, test_time, saturn);
        }

        [Test]
        public void SaturnPositionTest()
        {
            OrbitingBodyMathematics saturn = OrbitingBodyMathematics.generate_saturn();
            var test_time = new DateTime(2016, 2, 10);
            PositionTest(saturn, test_time, 250.37, 1.71, 10.002);
        }

        // ------------------------------------------------------------

        [Test]
        public void UranusOrbitalPeriodTest()
        {
            OrbitingBodyMathematics uranus = OrbitingBodyMathematics.generate_uranus();
            OrbitalPeriodTest(30685.4, uranus);
        }

        [Test]
        public void UranusMeanAnomalyTest()
        {
            OrbitingBodyMathematics uranus = OrbitingBodyMathematics.generate_uranus();
            var test_time = new DateTime(2016, 2, 10, 14, 18, 00);
            MeanAnomalyTest(3.666, test_time, uranus);
        }

        [Test]
        public void UranusEccentricAnomalyTest()
        {
            OrbitingBodyMathematics uranus = OrbitingBodyMathematics.generate_uranus();
            var test_time = new DateTime(2016, 2, 10, 14, 19, 00);
            EccentricAnomalyTest(3.644, test_time, uranus);
        }

        [Test]
        public void UranusPositionTest()
        {
            OrbitingBodyMathematics uranus = OrbitingBodyMathematics.generate_uranus();
            var test_time = new DateTime(2016, 2, 10);
            PositionTest(uranus, test_time, 20.32, -0.63, 19.924);
        }

        // ------------------------------------------------------------

        [Test]
        public void NeptuneOrbitalPeriodTest()
        {
            OrbitingBodyMathematics neptune = OrbitingBodyMathematics.generate_neptune();
            OrbitalPeriodTest(60189.0, neptune);
        }

        [Test]
        public void NeptuneMeanAnomalyTest()
        {
            OrbitingBodyMathematics neptune = OrbitingBodyMathematics.generate_neptune();
            var test_time = new DateTime(2016, 2, 10, 14, 31, 00);
            MeanAnomalyTest(5.085, test_time, neptune);
        }

        [Test]
        public void NeptuneEccentricAnomalyTest()
        {
            OrbitingBodyMathematics neptune = OrbitingBodyMathematics.generate_neptune();
            var test_time = new DateTime(2016, 2, 10, 14, 31, 00);
            EccentricAnomalyTest(5.077, test_time, neptune);
        }

        [Test]
        public void NeptunePositionTest()
        {
            OrbitingBodyMathematics neptune = OrbitingBodyMathematics.generate_neptune();
            var test_time = new DateTime(2016, 2, 10);
            PositionTest(neptune, test_time, 339.59, -0.81, 29.898);
        }

        // ------------------------------------------------------------
        [Test]
        public void EarthRotationTestMidnight()
        {
            OrbitingBodyMathematics earth
                = OrbitingBodyMathematics.generate_earth();
            var today = DateTime.Now;
            var test_time = new DateTime(
                today.Year, today.Month, today.Day, 0, 0, 0
            );
            RotationAbsoluteTest(0.5, test_time, earth);
        }

        [Test]
        public void EarthRotationTestSix()
        {
            OrbitingBodyMathematics earth
                = OrbitingBodyMathematics.generate_earth();
            var today = DateTime.Now;
            var test_time = new DateTime(
                today.Year, today.Month, today.Day, 6, 0, 0
            );
            RotationAbsoluteTest(0.75, test_time, earth);
        }

        [Test]
        public void EarthRotationTestNoon()
        {
            OrbitingBodyMathematics earth
                = OrbitingBodyMathematics.generate_earth();
            var today = DateTime.Now;
            var test_time = new DateTime(
                today.Year, today.Month, today.Day, 12, 0, 0
            );
            RotationAbsoluteTest(0.0, test_time, earth);
        }

        [Test]
        public void EarthRotationTestEighteen()
        {
            OrbitingBodyMathematics earth
                = OrbitingBodyMathematics.generate_earth();
            var today = DateTime.Now;
            var test_time = new DateTime(
                today.Year, today.Month, today.Day, 18, 0, 0
            );
            RotationAbsoluteTest(0.25, test_time, earth);
        }

        // ------------------------------------------------------------
        [Test]
        public void EarthRotationRelativeToSunTestMidnight()
        {
            OrbitingBodyMathematics earth
                = OrbitingBodyMathematics.generate_earth();
            var today = DateTime.Now;
            var test_time = new DateTime(
                today.Year, today.Month, today.Day, 0, 0, 0
            );
            RotationRelativeToSunTest(0, test_time, earth);
        }

        [Test]
        public void EarthRotationRelativeToSunTestSix()
        {
            OrbitingBodyMathematics earth
                = OrbitingBodyMathematics.generate_earth();
            var today = DateTime.Now;
            var test_time = new DateTime(
                today.Year, today.Month, today.Day, 6, 0, 0
            );
            RotationRelativeToSunTest(90, test_time, earth);
        }

        [Test]
        public void EarthRotationRelativeToSunTestNoon()
        {
            OrbitingBodyMathematics earth
                = OrbitingBodyMathematics.generate_earth();
            var today = DateTime.Now;
            var test_time = new DateTime(
                today.Year, today.Month, today.Day, 12, 0, 0
            );
            RotationRelativeToSunTest(180, test_time, earth);
        }

        [Test]
        public void EarthRotationRelativeToSunTestEighteen()
        {
            OrbitingBodyMathematics earth
                = OrbitingBodyMathematics.generate_earth();
            var today = DateTime.Now;
            var test_time = new DateTime(
                today.Year, today.Month, today.Day, 18, 0, 0
            );
            RotationRelativeToSunTest(90, test_time, earth);
        }
    }
}

