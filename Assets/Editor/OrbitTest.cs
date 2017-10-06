using UnityEngine;
using UnityEditor;
using System;
using NUnit.Framework;

namespace SpaceBattles
{
    public class OrbitTest
    {
        public static String
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
        /// <param name="expected_distance">In AU</param>
        public static void
        PositionTest(OrbitingBodyMathematics planet,
                     DateTime test_time,
                     double expected_longitude,
                     double expected_latitude,
                     double expected_distance)
        {
            string expected_string
                = "\nExpected: longitude " + expected_longitude.ToString("F5")
                + " degrees, latitude " + expected_latitude.ToString("F5")
                + " degrees, distance " + expected_distance.ToString("F5")
                + " AU";
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
            

            double latitude_error = Math.Abs(longlatdist.y - expected_latitude);
            double acceptable_latitude_error = 0.01;
            string latitude_error_message = differing_results_error_message(
                Convert.ToString(longlatdist.y),
                Convert.ToString(expected_latitude),
                "degrees"
            );
            

            double distance_error = Math.Abs(longlatdist.z - expected_distance);
            double acceptable_distance_error = 0.00001;
            string distance_error_message = differing_results_error_message(
                Convert.ToString(longlatdist.z),
                Convert.ToString(expected_distance),
                "AU"
            );

            Debug.Log(
                "Longitude Error: " + longitude_error + " degrees, "
                + "Latitude Error: " + latitude_error + " degrees, "
                + "Distance Error: " + distance_error + " AU"
            );

            Assert.LessOrEqual(longitude_error, acceptable_longitude_error,
                               "Longitude Error: " + longitude_error_message + expected_string);
            Assert.LessOrEqual(latitude_error, acceptable_latitude_error,
                               "Latitude Error: " + latitude_error_message + expected_string);
            Assert.LessOrEqual(distance_error, acceptable_distance_error,
                               "Distance Error: " + distance_error_message + expected_string);
        }

        public static void
        CompoundPositionTest (OrbitingBodyMathematics planet,
                              CompoundPositionTester[] tests)
        {
            bool AnyTestsFailed = false;
            foreach (CompoundPositionTester test in tests)
            {
                test.Planet = planet;
                try
                {
                    test.Test();
                }
                catch (AssertionException failedTestException)
                {
                    Debug.Log(failedTestException.Message);
                    AnyTestsFailed = true;
                }
            }
            Assert.IsFalse(AnyTestsFailed, "Compound test failed");
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

        [Test]
        public void MercuryCompoundPositionTest()
        {
            OrbitingBodyMathematics Mercury = OrbitingBodyMathematics.generate_mercury();
            CompoundPositionTester[] Tests =
            {
                new CompoundPositionTester(new DateTime(1968,12,3),243.9,-1.93,0.464),
                new CompoundPositionTester(new DateTime(1968,11,2),127.03,6.88,0.327),
                new CompoundPositionTester(new DateTime(1968,5,18),166.95,6.14,0.370),
                new CompoundPositionTester(new DateTime(1968,3,28),282.52,-5.71,0.455),
                new CompoundPositionTester(new DateTime(1968,10,17),29.83,-2.2,0.325),
                new CompoundPositionTester(new DateTime(1968,4,27),47.01,-0.12,0.315),
                new CompoundPositionTester(new DateTime(1968,11,4),138.06,7.00,0.337),
                new CompoundPositionTester(new DateTime(1968,7,4),314.24,-6.99,0.417),
                new CompoundPositionTester(new DateTime(1968,12,27),314.45,-6.99,0.417),
                new CompoundPositionTester(new DateTime(1968,4,16),349.09,-6.01,0.368),
                new CompoundPositionTester(new DateTime(1985,4,11),218.18,1.22,0.441),
                new CompoundPositionTester(new DateTime(1985,9,27),195.41,3.80,0.410),
                new CompoundPositionTester(new DateTime(1985,3,13),85.48,4.26,0.308),
                new CompoundPositionTester(new DateTime(1985,4,17),235.92,-0.95,0.459),
                new CompoundPositionTester(new DateTime(1985,5,11),304.89,-6.82,0.430),
                new CompoundPositionTester(new DateTime(1985,12,30),215.37,1.56,0.438),
                new CompoundPositionTester(new DateTime(1985,6,16),127.96,6.89,0.328),
                new CompoundPositionTester(new DateTime(1985,1,31),268.92,-4.59,0.464),
                new CompoundPositionTester(new DateTime(1985,3,30),176.28,5.52,0.383),
                new CompoundPositionTester(new DateTime(1985,6,4),54.45,0.77,0.312),
                new CompoundPositionTester(new DateTime(2008,10,19),85.92,4.28,0.308),
                new CompoundPositionTester(new DateTime(2008,7,5),345.85,-6.22,0.3737),
                new CompoundPositionTester(new DateTime(2008,8,5),159.05,6.56,0.360),
                new CompoundPositionTester(new DateTime(2008,9,27),329.74,-6.87,0.396),
                new CompoundPositionTester(new DateTime(2008,4,20),48.24,-0.02,0.314),
                new CompoundPositionTester(new DateTime(2008,12,25),333.74,-6.87,0.396),
                new CompoundPositionTester(new DateTime(2008,7,21),73.15,2.95,0.308),
                new CompoundPositionTester(new DateTime(2008,5,16),188.06,4.55,0.399),
                new CompoundPositionTester(new DateTime(2008,7,9),4.29,-4.89,0.350),
                new CompoundPositionTester(new DateTime(2008,1,11),345.58,-6.24,0.373),
                new CompoundPositionTester(new DateTime(1987,6,20),242.36,-1.72,0.463),
                new CompoundPositionTester(new DateTime(1987,7,22),342.69,-6.38,0.377),
                new CompoundPositionTester(new DateTime(1987,5,15),93.51,4.99,0.310),
                new CompoundPositionTester(new DateTime(1987,3,30),258.72,-3.57,0.467),
                new CompoundPositionTester(new DateTime(1987,11,5),81.36,3.85,0.308),
                new CompoundPositionTester(new DateTime(1987,5,21),129.37,6.92,0.329),
                new CompoundPositionTester(new DateTime(1987,10,23),5.76,-4.74,0.348),
                new CompoundPositionTester(new DateTime(1987,11,9),106.25,5.95,0.314),
                new CompoundPositionTester(new DateTime(1987,12,31),293.32,-6.36,0.444),
                new CompoundPositionTester(new DateTime(1987,12,2),209.82,2.22,0.430),
                new CompoundPositionTester(new DateTime(1973,3,23),206.80,2.55,0.427),
                new CompoundPositionTester(new DateTime(1973,9,3),161.09,6.45,0.363),
                new CompoundPositionTester(new DateTime(1973,3,20),196.66,3.66,0.412),
                new CompoundPositionTester(new DateTime(1973,2,9),351.91,-5.83,0.365),
                new CompoundPositionTester(new DateTime(1973,11,19),101.23,5.62,0.312),
                new CompoundPositionTester(new DateTime(1973,1,4),237.01,-1.10,0.459),
                new CompoundPositionTester(new DateTime(1973,2,24),75.63,3.26,0.308),
                new CompoundPositionTester(new DateTime(1973,1,28),306.16,-6.86,0.428),
                new CompoundPositionTester(new DateTime(1973,6,21),213.33,1.79,0.435),
                new CompoundPositionTester(new DateTime(1973,4,28),312.97,-6.98,0.419)
            };
            CompoundPositionTest(Mercury, Tests);
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

        [Test]
        public void VenusCompoundPositionTest()
        {
            OrbitingBodyMathematics Venus = OrbitingBodyMathematics.generate_venus();
            CompoundPositionTester[] Tests =
            {
                new CompoundPositionTester(new DateTime(1996,3,2),97.67,1.22,0.719),
                new CompoundPositionTester(new DateTime(1996,12,6),185.80,3.21,0.720),
                new CompoundPositionTester(new DateTime(1996,4,7),156.11,3.34,0.719),
                new CompoundPositionTester(new DateTime(1996,1,23),34.93,-2.26,0.724),
                new CompoundPositionTester(new DateTime(1996,4,21),178.83,3.32,0.720),
                new CompoundPositionTester(new DateTime(1996,8,17),6.69,-3.19,0.726),
                new CompoundPositionTester(new DateTime(1996,9,30),77.16,0.03,0.720),
                new CompoundPositionTester(new DateTime(1996,10,21),111.12,1.92,0.719),
                new CompoundPositionTester(new DateTime(1996,5,22),228.72,1.59,0.724),
                new CompoundPositionTester(new DateTime(1996,11,23),164.73,3.39,0.719)
            };
            CompoundPositionTest(Venus, Tests);
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

        [Test]
        public void EarthCompoundPositionTest()
        {
            OrbitingBodyMathematics Earth = OrbitingBodyMathematics.generate_earth();
            CompoundPositionTester[] Tests =
            {
                new CompoundPositionTester(new DateTime(2012,7,21),298.62,0,1.016),
                new CompoundPositionTester(new DateTime(2012,4,6),196.58,0,1.001),
                new CompoundPositionTester(new DateTime(2012,7,8),286.22,0,1.017),
                new CompoundPositionTester(new DateTime(2012,9,23),360.35,0,1.003),
                new CompoundPositionTester(new DateTime(2012,2,14),144.67,0,0.987),
                new CompoundPositionTester(new DateTime(2012,11,25),63.1,0,0.987),
                new CompoundPositionTester(new DateTime(2012,3,14),173.79,0,0.994),
                new CompoundPositionTester(new DateTime(2012,12,22),90.52,0,0.984),
                new CompoundPositionTester(new DateTime(2012,6,9),258.55,0,1.015),
                new CompoundPositionTester(new DateTime(2012,12,16),84.41,0,0.984)
            };
            CompoundPositionTest(Earth, Tests);
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

        [Test]
        public void MarsCompoundPositionTest()
        {
            OrbitingBodyMathematics Mars = OrbitingBodyMathematics.generate_mars();
            CompoundPositionTester[] Tests =
            {
                new CompoundPositionTester(new DateTime(1974,7,9),157.26,1.76,1.666),
                new CompoundPositionTester(new DateTime(1974,12,19),232.42,-0.1,1.543),
                new CompoundPositionTester(new DateTime(1974,11,18),217.05,0.39,1.581),
                new CompoundPositionTester(new DateTime(1974,8,26),178.33,1.44,1.653),
                new CompoundPositionTester(new DateTime(1974,1,17),77.5,0.88,1.54),
                new CompoundPositionTester(new DateTime(1974,7,23),163.37,1.69,1.664),
                new CompoundPositionTester(new DateTime(1974,5,4),128.23,1.82,1.647),
                new CompoundPositionTester(new DateTime(1974,1,14),75.96,0.83,1.536),
                new CompoundPositionTester(new DateTime(1974,9,27),192.69,1.1,1.632),
                new CompoundPositionTester(new DateTime(1974,2,2),85.56,1.1,1.56)
            };
            CompoundPositionTest(Mars, Tests);
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

        [Test]
        public void JupiterCompoundPositionTest()
        {
            OrbitingBodyMathematics Jupiter = OrbitingBodyMathematics.generate_jupiter();
            CompoundPositionTester[] Tests =
            {
                new CompoundPositionTester(new DateTime(2007,7,28),259.14,0.48,5.306),
                new CompoundPositionTester(new DateTime(2007,12,15),270.42,0.23,5.258),
                new CompoundPositionTester(new DateTime(2007,6,16),255.8,0.55,5.319),
                new CompoundPositionTester(new DateTime(2007,4,13),250.73,0.65,5.339),
                new CompoundPositionTester(new DateTime(2007,3,10),248.06,0.7,5.349),
                new CompoundPositionTester(new DateTime(2007,7,15),258.11,0.5,5.31),
                new CompoundPositionTester(new DateTime(2007,7,28),259.14,0.48,5.306),
                new CompoundPositionTester(new DateTime(2007,6,6),255,0.56,5.322),
                new CompoundPositionTester(new DateTime(2007,3,13),248.29,0.7,5.348),
                new CompoundPositionTester(new DateTime(2007,9,15),263.07,0.39,5.29)
            };
            CompoundPositionTest(Jupiter, Tests);
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

        [Test]
        public void SaturnCompoundPositionTest()
        {
            OrbitingBodyMathematics Saturn = OrbitingBodyMathematics.generate_saturn();
            CompoundPositionTester[] Tests =
            {
                new CompoundPositionTester(new DateTime(2015,4,29),241.62,1.96,9.963),
                new CompoundPositionTester(new DateTime(2015,1,4),238.1,2.05,9.944),
                new CompoundPositionTester(new DateTime(2015,9,15),245.87,1.84,9.983),
                new CompoundPositionTester(new DateTime(2015,5,6),241.84,1.96,9.964),
                new CompoundPositionTester(new DateTime(2015,11,26),248.06,1.78,9.993),
                new CompoundPositionTester(new DateTime(2015,11,20),247.88,1.79,9.992),
                new CompoundPositionTester(new DateTime(2015,2,24),239.66,2.01,9.953),
                new CompoundPositionTester(new DateTime(2015,11,3),247.36,1.8,9.99),
                new CompoundPositionTester(new DateTime(2015,9,30),246.33,1.83,9.985),
                new CompoundPositionTester(new DateTime(2015,4,17),241.26,1.97,9.961)
            };
            CompoundPositionTest(Saturn, Tests);
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

        [Test]
        public void UranusCompoundPositionTest()
        {
            OrbitingBodyMathematics Uranus = OrbitingBodyMathematics.generate_uranus();
            CompoundPositionTester[] Tests =
            {
                new CompoundPositionTester(new DateTime(1961,10,19),146.62,0.74,18.311),
                new CompoundPositionTester(new DateTime(1961,2,1),143.26,0.73,18.33),
                new CompoundPositionTester(new DateTime(1961,4,11),144.15,0.73,18.325),
                new CompoundPositionTester(new DateTime(1961,6,5),144.86,0.73,18.32),
                new CompoundPositionTester(new DateTime(1961,3,1),143.62,0.73,18.328),
                new CompoundPositionTester(new DateTime(1961,12,25),147.49,0.74,18.306),
                new CompoundPositionTester(new DateTime(1961,12,6),147.24,0.74,18.308),
                new CompoundPositionTester(new DateTime(1961,8,26),145.92,0.74,18.315),
                new CompoundPositionTester(new DateTime(1961,2,24),143.56,0.73,18.328),
                new CompoundPositionTester(new DateTime(1961,3,12),143.77,0.73,18.327)
            };
            CompoundPositionTest(Uranus, Tests);
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

        [Test]
        public void NeptuneCompoundPositionTest()
        {
            OrbitingBodyMathematics Neptune = OrbitingBodyMathematics.generate_neptune();
            CompoundPositionTester[] Tests =
            {
                new CompoundPositionTester(new DateTime(1960,7,19),218.2,1.77,30.323),
                new CompoundPositionTester(new DateTime(1960,1,23),217.15,1.77,30.322),
                new CompoundPositionTester(new DateTime(1960,6,28),218.08,1.77,30.323),
                new CompoundPositionTester(new DateTime(1960,6,15),218,1.77,30.323),
                new CompoundPositionTester(new DateTime(1960,4,16),217.65,1.77,30.322),
                new CompoundPositionTester(new DateTime(1960,4,3),217.57,1.77,30.322),
                new CompoundPositionTester(new DateTime(1960,1,16),217.11,1.77,30.322),
                new CompoundPositionTester(new DateTime(1960,10,16),218.73,1.77,30.323),
                new CompoundPositionTester(new DateTime(1960,9,16),218.55,1.77,30.323),
                new CompoundPositionTester(new DateTime(1960,5,27),217.89,1.77,30.323)
            };
            CompoundPositionTest(Neptune, Tests);
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

        public class CompoundPositionTester
        {
            public OrbitingBodyMathematics Planet;
            public DateTime TestTime;
            public double ExpectedLongitude;
            public double ExpectedLatitude;
            public double ExpectedDistance;

            public
            CompoundPositionTester
                (DateTime testTime,
                 double expectedLongitude,
                 double expectedLatitude,
                 double expectedDistance)
            {
                TestTime = testTime;
                ExpectedLongitude = expectedLongitude;
                ExpectedLatitude = expectedLatitude;
                ExpectedDistance = expectedDistance;
            }

            public void Test ()
            {
                MyContract.RequireFieldNotNull(Planet, "Planet");
                PositionTest(
                    Planet,
                    TestTime,
                    ExpectedLongitude,
                    ExpectedLatitude,
                    ExpectedDistance
                );
            }
        }
    }
}

