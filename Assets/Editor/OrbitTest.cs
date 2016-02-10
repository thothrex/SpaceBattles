using UnityEngine;
using UnityEditor;
using System;
using NUnit.Framework;

namespace SpaceBattles
{
    public class OrbitTest
    {
        public OrbitingBody generate_mercury ()
        {
            // from http://www.astropixels.com/ephemeris/astrocal/astrocal2016gmt.html
            var last_mercury_perihelion = new DateTime(2016, 1, 8, 18, 0, 0);
            // from http://nssdc.gsfc.nasa.gov/planetary/factsheet/mercuryfact.html
            return new OrbitingBody(57.91, 0.20563069, last_mercury_perihelion, 7.00487, 48.33167, 77.45645, 0.3301);
        }

        public OrbitingBody generate_venus()
        {
            // from http://www.astropixels.com/ephemeris/astrocal/astrocal2015gmt.html
            var last_venus_perihelion = new DateTime(2015, 11, 29, 6, 0, 0);
            // from http://nssdc.gsfc.nasa.gov/planetary/factsheet/venusfact.html
            return new OrbitingBody(108.21, 0.00677323, last_venus_perihelion, 3.39471, 76.68069, 131.53298, 4.8676);
        }

        public OrbitingBody generate_earth()
        {
            // from http://aa.usno.navy.mil/data/docs/EarthSeasons.php
            var last_earth_perihelion = new DateTime(2016, 1, 2, 22, 49, 0);
            // from http://nssdc.gsfc.nasa.gov/planetary/factsheet/earthfact.html
            return new OrbitingBody(149.60, 0.01671022, last_earth_perihelion, 0.00005, -11.26064, 102.94719, 5.9726);
        }

        public OrbitingBody generate_mars()
        {
            // from http://www.astropixels.com/ephemeris/astrocal/astrocal2014gmt.html
            var last_mars_perihelion = new DateTime(2014, 12, 12, 12, 0, 0);
            // from http://nssdc.gsfc.nasa.gov/planetary/factsheet/marsfact.html
            return new OrbitingBody(227.92, 0.09341233, last_mars_perihelion, 1.85061, 49.57854, 336.04084, 0.64174);
        }

        public OrbitingBody generate_jupiter()
        {
            // from http://www.astropixels.com/ephemeris/astrocal/astrocal2011gmt.html
            var last_jupiter_perihelion = new DateTime(2011, 3, 16, 11, 0, 0);
            // from http://nssdc.gsfc.nasa.gov/planetary/factsheet/jupiterfact.html
            return new OrbitingBody(778.57, 0.04839266, last_jupiter_perihelion, 1.30530, 100.55615, 14.75385, 1898.3);
        }

        public OrbitingBody generate_saturn()
        {
            // from https://www.princeton.edu/~willman/planetary_systems/Sol/Saturn/
            // source seem to disagree
            var last_saturn_perihelion = new DateTime(2003, 7, 11, 13, 30, 0);
            // from http://nssdc.gsfc.nasa.gov/planetary/factsheet/saturnfact.html
            return new OrbitingBody(1433.53, 0.05415060, last_saturn_perihelion, 2.48446, 113.71504, 92.43194, 568.36);
        }

        public OrbitingBody generate_uranus()
        {
            // from https://www.princeton.edu/~willman/planetary_systems/Sol/Uranus/
            var last_uranus_perihelion = new DateTime(1966, 9, 9);
            // from http://nssdc.gsfc.nasa.gov/planetary/factsheet/uranusfact.html
            return new OrbitingBody(2872.46, 0.04716771, last_uranus_perihelion, 0.76986, 74.22988, 170.96424, 86.816);
        }

        public OrbitingBody generate_neptune()
        {
            // from https://www.princeton.edu/~willman/planetary_systems/Sol/Neptune/
            var last_uranus_perihelion = new DateTime(2046, 11, 13);
            // from http://nssdc.gsfc.nasa.gov/planetary/factsheet/neptunefact.html
            return new OrbitingBody(4495.06, 0.00858587, last_uranus_perihelion, 1.76917, 131.72169, 44.97135, 102.42);
        }

        public String
        differing_results_error_message (String expected,
                                         String actual,
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
                           OrbitingBody planet)
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
                        OrbitingBody planet)
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
                            OrbitingBody planet)
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
        /// long lat and distance data retrieved from http://omniweb.gsfc.nasa.gov/cgi/models/planet.cgi
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="test_time"></param>
        /// <param name="expected_longitude"></param>
        /// <param name="expected_latitude"></param>
        /// <param name="expected_distance"></param>
        public void
        PositionTest(OrbitingBody planet,
                     DateTime test_time,
                     double expected_longitude,
                     double expected_latitude,
                     double expected_distance)
        {
            Vector3 coordinates = planet.current_location(test_time);
            Vector3 longlatdist = planet.current_longlatdist(test_time);

            Console.Write(coordinates.ToString());
            Console.Write(longlatdist.ToString());

            double longitude_error = Math.Abs(longlatdist.x - expected_longitude);
            double acceptable_longitude_error = 0.05;
            string longitude_error_message = differing_results_error_message(
                Convert.ToString(expected_longitude),
                Convert.ToString(longlatdist.x),
                "degrees"
            );
            Assert.LessOrEqual(longitude_error, acceptable_longitude_error,
                               "Longitude Error: " + longitude_error_message);

            double latitude_error = Math.Abs(longlatdist.y - expected_latitude);
            double acceptable_latitude_error = 0.01;
            string latitude_error_message = differing_results_error_message(
                Convert.ToString(expected_latitude),
                Convert.ToString(longlatdist.y),
                "degrees"
            );
            Assert.LessOrEqual(latitude_error, acceptable_latitude_error,
                               "Latitude Error: " + latitude_error_message);

            double distance_error = Math.Abs(longlatdist.z - expected_distance);
            double acceptable_distance_error = 0.00001;
            string distance_error_message = differing_results_error_message(
                Convert.ToString(expected_distance),
                Convert.ToString(longlatdist.z),
                "AU"
            );
            Assert.LessOrEqual(distance_error, acceptable_distance_error,
                               "Distance Error: " + distance_error_message);
        }

        // --------------------------------------

        [Test]
        public void MercuryOrbitalPeriodTest()
        {
            OrbitingBody mercury = generate_mercury();
            OrbitalPeriodTest(87.969, mercury);
        }

        [Test]
        public void MercuryMeanAnomalyTest()
        {
            OrbitingBody mercury = generate_mercury();
            var test_time = new DateTime(2016, 2, 10, 11, 48, 00);
            MeanAnomalyTest(2.34, test_time, mercury);
        }

        [Test]
        public void MercuryEccentricAnomalyTest()
        {
            OrbitingBody mercury = generate_mercury();
            var test_time = new DateTime(2016, 2, 10, 11, 49, 0);
            EccentricAnomalyTest(2.468, test_time, mercury);
        }

        [Test]
        public void MercuryPositionTest()
        {
            OrbitingBody mercury = generate_mercury();
            var test_time = new DateTime(2016, 2, 10);
            PositionTest(mercury, test_time, 224.43, 0.13, 0.451);
        }

        // ------------------------------------------------------------
        [Test]
        public void VenusOrbitalPeriodTest()
        {
            OrbitingBody venus = generate_venus();
            OrbitalPeriodTest(224.701, venus);
        }

        [Test]
        public void VenusMeanAnomalyTest()
        {
            OrbitingBody venus = generate_venus();
            var test_time = new DateTime(2016, 2, 10, 12, 17, 00);
            MeanAnomalyTest(2.045, test_time, venus);
        }

        [Test]
        public void VenusEccentricAnomalyTest()
        {
            OrbitingBody venus = generate_venus();
            var test_time = new DateTime(2016, 2, 10, 12, 18, 00);
            EccentricAnomalyTest(2.051, test_time, venus);
        }

        [Test]
        public void VenusPositionTest()
        {
            OrbitingBody venus = generate_venus();
            var test_time = new DateTime(2016, 2, 10);
            PositionTest(venus, test_time, 249.01, 0.46, 0.726);
        }

        // ------------------------------------------------------------

        [Test]
        public void EarthOrbitalPeriodTest()
        {
            OrbitingBody earth = generate_earth();
            OrbitalPeriodTest(365.25636, earth);
        }

        [Test]
        public void EarthMeanAnomalyTest1()
        {
            OrbitingBody earth = generate_earth();
            var test_time = new DateTime(2016, 2, 9, 14, 49, 00);
            MeanAnomalyTest(0.6441, test_time, earth);
        }

        [Test]
        public void EarthMeanAnomalyTest2()
        {
            OrbitingBody earth = generate_earth();
            var test_time = new DateTime(2016, 2, 10, 9, 54, 00);
            MeanAnomalyTest(0.6577, test_time, earth);
        }

        [Test]
        public void EarthEccentricAnomalyTest()
        {
            OrbitingBody earth = generate_earth();
            var test_time = new DateTime(2016, 2, 10, 13, 25, 00);
            EccentricAnomalyTest(0.6705, test_time, earth);
        }

        [Test]
        public void EarthPositionTest()
        {
            OrbitingBody earth = generate_earth();
            var test_time = new DateTime(2016, 2, 10);
            PositionTest(earth, test_time, 140.65, 0.0, 0.987);
        }

        // ------------------------------------------------------------

        [Test]
        public void MarsOrbitalPeriodTest()
        {
            OrbitingBody mars = generate_mars();
            OrbitalPeriodTest(686.980, mars);
        }

        [Test]
        public void MarsMeanAnomalyTest()
        {
            OrbitingBody mars = generate_mars();
            var test_time = new DateTime(2016, 2, 10, 12, 32, 00);
            MeanAnomalyTest(3.887, test_time, mars);
        }

        [Test]
        public void MarsEccentricAnomalyTest()
        {
            OrbitingBody mars = generate_mars();
            var test_time = new DateTime(2016, 2, 10, 12, 33, 00);
            EccentricAnomalyTest(3.828, test_time, mars);
        }

        [Test]
        public void MarsPositionTest()
        {
            OrbitingBody mars = generate_mars();
            var test_time = new DateTime(2016, 2, 10);
            PositionTest(mars, test_time, 192.34, 1.13, 1.634);
        }

        // ------------------------------------------------------------

        [Test]
        public void JupiterOrbitalPeriodTest()
        {
            OrbitingBody jupiter = generate_jupiter();
            OrbitalPeriodTest(4332.589, jupiter);
        }

        [Test]
        public void JupiterMeanAnomalyTest()
        {
            OrbitingBody jupiter = generate_jupiter();
            var test_time = new DateTime(2016, 2, 10, 13, 12, 00);
            MeanAnomalyTest(2.599, test_time, jupiter);
        }

        [Test]
        public void JupiterEccentricAnomalyTest()
        {
            OrbitingBody jupiter = generate_jupiter();
            var test_time = new DateTime(2016, 2, 10, 13, 27, 00);
            EccentricAnomalyTest(2.623, test_time, jupiter);
        }

        [Test]
        public void JupiterPositionTest()
        {
            OrbitingBody jupiter = generate_jupiter();
            var test_time = new DateTime(2016, 2, 10);
            PositionTest(jupiter, test_time, 166.22, 1.19, 5.418);
        }

        // ------------------------------------------------------------

        [Test]
        public void SaturnOrbitalPeriodTest()
        {
            OrbitingBody saturn = generate_saturn();
            OrbitalPeriodTest(10759.22, saturn);
        }

        [Test]
        public void SaturnMeanAnomalyTest()
        {
            OrbitingBody saturn = generate_saturn();
            var test_time = new DateTime(2016, 2, 10, 13, 43, 00);
            MeanAnomalyTest(2.685, test_time, saturn);
        }

        [Test]
        public void SaturnEccentricAnomalyTest()
        {
            OrbitingBody saturn = generate_saturn();
            var test_time = new DateTime(2016, 2, 10, 13, 44, 00);
            EccentricAnomalyTest(2.707, test_time, saturn);
        }

        [Test]
        public void SaturnPositionTest()
        {
            OrbitingBody saturn = generate_saturn();
            var test_time = new DateTime(2016, 2, 10);
            PositionTest(saturn, test_time, 250.37, 1.71, 10.002);
        }

        // ------------------------------------------------------------

        [Test]
        public void UranusOrbitalPeriodTest()
        {
            OrbitingBody uranus = generate_uranus();
            OrbitalPeriodTest(30685.4, uranus);
        }

        [Test]
        public void UranusMeanAnomalyTest()
        {
            OrbitingBody uranus = generate_uranus();
            var test_time = new DateTime(2016, 2, 10, 14, 18, 00);
            MeanAnomalyTest(3.666, test_time, uranus);
        }

        [Test]
        public void UranusEccentricAnomalyTest()
        {
            OrbitingBody uranus = generate_uranus();
            var test_time = new DateTime(2016, 2, 10, 14, 19, 00);
            EccentricAnomalyTest(3.644, test_time, uranus);
        }

        [Test]
        public void UranusPositionTest()
        {
            OrbitingBody uranus = generate_uranus();
            var test_time = new DateTime(2016, 2, 10);
            PositionTest(uranus, test_time, 20.32, -0.63, 19.924);
        }

        // ------------------------------------------------------------

        [Test]
        public void NeptuneOrbitalPeriodTest()
        {
            OrbitingBody neptune = generate_neptune();
            OrbitalPeriodTest(60189.0, neptune);
        }

        [Test]
        public void NeptuneMeanAnomalyTest()
        {
            OrbitingBody neptune = generate_neptune();
            var test_time = new DateTime(2016, 2, 10, 14, 31, 00);
            MeanAnomalyTest(5.085, test_time, neptune);
        }

        [Test]
        public void NeptuneEccentricAnomalyTest()
        {
            OrbitingBody neptune = generate_neptune();
            var test_time = new DateTime(2016, 2, 10, 14, 31, 00);
            EccentricAnomalyTest(5.077, test_time, neptune);
        }

        [Test]
        public void NeptunePositionTest()
        {
            OrbitingBody neptune = generate_neptune();
            var test_time = new DateTime(2016, 2, 10);
            PositionTest(neptune, test_time, 339.59, -0.81, 29.898);
        }
    }
}

