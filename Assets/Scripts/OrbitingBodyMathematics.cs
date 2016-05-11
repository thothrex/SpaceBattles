﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattles
{
    /**
     * As I am not an astrophysicist,
     * I am using the very helpful guide at http://www.stjarnhimlen.se/comp/tutorial.html
     * to direct and inform my calculations and structure here.
     *
     * Stats taken from the detailed pages of http://nssdc.gsfc.nasa.gov/planetary/factsheet/
     */
    public class OrbitingBodyMathematics
    {
        static public readonly double MASS_SCALE_TO_KG = 1000000000000000000000000.0; // 10^24
        static public readonly double DISTANCE_SCALE_TO_METRES = 1000000000.0; // 10^6km -> metres
        static public readonly double TICKS_TO_SECONDS = 10000000; // 10,000,000 ticks per second
        static public readonly double DEG_TO_RAD = Math.PI / 180.0;
        static public readonly double AU = 149597870700 / DISTANCE_SCALE_TO_METRES; // AU is in metres, so normalise
        static public readonly double EARTH_YEAR_IN_EPOCH_UNITS = 31558149.7635456; // seconds
        static public readonly double CONSTANT_OF_GRAVITATION = 0.0000000000667408; // m^3 kg^-1 s^-2
        static public readonly double FULL_ROTATION_ANGLE = 2 * Math.PI; // radians
        static public readonly double ACCEPTABLE_ECCENTRIC_ANOMALY_ERROR = 0.0001;
        public enum ORBITING_BODY {SUN, MERCURY, VENUS, EARTH, MOON, MARS, JUPITER, SATURN, URANUS, NEPTUNE};
        // Primary orbital elements
        private double semi_major_axis; // 10^6km
        private double eccentricity;
        private DateTime time_last_at_periapsis;

        private double inclination; // degrees
        private double longitude_of_ascending_node; // degrees
        private double argument_of_periapsis; // degrees - calculated from longitude

        public readonly double mass; // 10^24kg
        private double mass_in_solar_masses; // mass / SOLAR_MASS
        public readonly double SOLAR_MASS = 1988500.0; // 10^24kg
        private double longitude_of_periapsis;

        // Derived elements
        private double periapsis_distance; // 10^6km
        private double apoapsis_distance; // 10^6 km
        private double orbital_period; // in seconds (SI Unit time)
        private double mean_angular_motion; // in degrees/radians per unit time (e.g. degrees/second)

        public OrbitingBodyMathematics(double semi_major_axis,
                             double eccentricity,
                             DateTime time_last_at_periapsis,
                             double inclination,
                             double longitude_of_ascending_node,
                             double longitude_of_periapsis, // argument_of_periapsis
                             double mass)
        {
            this.semi_major_axis = semi_major_axis;
            this.eccentricity = eccentricity;
            this.time_last_at_periapsis = time_last_at_periapsis;
            this.inclination = inclination;
            this.longitude_of_ascending_node = longitude_of_ascending_node;
            this.longitude_of_periapsis = longitude_of_periapsis;
            this.mass = mass;

            this.mass_in_solar_masses = mass / SOLAR_MASS;
            this.orbital_period = 2 * Math.PI * Math.Sqrt(
                  Math.Pow(semi_major_axis * DISTANCE_SCALE_TO_METRES, 3.0)
                / (CONSTANT_OF_GRAVITATION * (SOLAR_MASS * MASS_SCALE_TO_KG + mass * MASS_SCALE_TO_KG))
            );

            setup();
        }

        public OrbitingBodyMathematics(double semi_major_axis,
                            double eccentricity,
                            DateTime time_last_at_periapsis,
                            double inclination,
                            double longitude_of_ascending_node,
                            double longitude_of_periapsis, // argument_of_periapsis
                            double mass,
                            OrbitingBodyMathematics orbiting_target,
                            double orbiting_target_mass)
        {
            this.semi_major_axis = semi_major_axis;
            this.eccentricity = eccentricity;
            this.time_last_at_periapsis = time_last_at_periapsis;
            this.inclination = inclination;
            this.longitude_of_ascending_node = longitude_of_ascending_node;
            this.longitude_of_periapsis = longitude_of_periapsis;
            this.mass = mass;

            this.orbital_period = 2 * Math.PI * Math.Sqrt(
                  Math.Pow(semi_major_axis * DISTANCE_SCALE_TO_METRES, 3.0)
                / (CONSTANT_OF_GRAVITATION * (orbiting_target_mass * MASS_SCALE_TO_KG + mass * MASS_SCALE_TO_KG))
            );

            setup();
        }

        private void setup()
        {
            this.periapsis_distance = semi_major_axis * (1 - eccentricity);
            this.apoapsis_distance = semi_major_axis * (1 + eccentricity);
            this.argument_of_periapsis = longitude_of_periapsis - longitude_of_ascending_node;
            this.mean_angular_motion = FULL_ROTATION_ANGLE / orbital_period;
        }


        public TimeSpan time_since_periapsis(DateTime current_time)
        {
            return current_time - time_last_at_periapsis;
        }

        /// <summary>
        /// (my best guess) calculates the current angle from the periapsis 
        /// https://en.wikipedia.org/wiki/Mean_anomaly
        /// </summary>
        /// <param name="current_time"></param>
        /// <returns>A value in radians</returns>
        public double mean_anomaly(DateTime current_time)
        {
            return normalise_radians(
                  mean_angular_motion
                * (time_since_periapsis(current_time).Ticks / TICKS_TO_SECONDS)
            );
        }

        /// <summary>
        /// Returns a value in radians
        /// https://en.wikipedia.org/wiki/Eccentric_anomaly
        /// </summary>
        /// <param name="current_time"></param>
        /// <param name="max_iterations"></param>
        /// <returns></returns>
        public double eccentric_anomaly(DateTime current_time, int max_iterations)
        {
            double M = mean_anomaly(current_time);
            double current_approximation
                = M + eccentricity * Math.Sin(M) * (1.0 + eccentricity * Math.Cos(M));
            current_approximation = normalise_radians(current_approximation);

            for (int i = 0; i < max_iterations; i++)
            {
                double prev_approximation = current_approximation;
                current_approximation
                    = prev_approximation
                    - (
                        (prev_approximation - eccentricity * Math.Sin(prev_approximation) - M)
                        /
                        (1.0 - eccentricity * Math.Cos(prev_approximation))
                       );
                current_approximation = normalise_radians(current_approximation);
                if (  Math.Abs(prev_approximation - current_approximation)
                    < ACCEPTABLE_ECCENTRIC_ANOMALY_ERROR) {
                    break;
                }
            }
            return current_approximation;
        }

        private double normalise_radians(double input_radians)
        {
            double normalised_value = input_radians;
            while (normalised_value < 0) { normalised_value += 2 * Math.PI; }
            return normalised_value % (2 * Math.PI);
        }

        /// <summary>
        /// Mainly for testing purposes - should still be safe to use otherwise.
        /// Returns a value in seconds
        /// </summary>
        /// <returns>double</returns>
        public double get_orbital_period()
        {
            return orbital_period;
        }


        public Vector3 current_location(DateTime current_time)
        {
            double E = eccentric_anomaly(current_time, 100);
            double x_in_plane = semi_major_axis * (Math.Cos(E) - eccentricity);
            double y_in_plane
                = semi_major_axis
                * (Math.Sqrt(1.0 - Math.Pow(eccentricity, 2)) * Math.Sin(E));

            // v = true anomaly
            double v = Math.Atan2(y_in_plane, x_in_plane);
            // r = distance from centre
            double r = Math.Sqrt(Math.Pow(x_in_plane, 2) + Math.Pow(y_in_plane, 2));
            double N = longitude_of_ascending_node * DEG_TO_RAD;
            double i = inclination * DEG_TO_RAD; // i = inclination to ecliptic from degrees
            double w = argument_of_periapsis * DEG_TO_RAD;

            double sinvw = Math.Sin(v + w);
            double sinN = Math.Sin(N);
            double cosN = Math.Cos(N);
            double cosvw = Math.Cos(v + w);
            double cosi = Math.Cos(i);

            // centred coordinates around system being used elsewhere
            // typically heliocentric, but could be terracentric for the moon
            double centred_x = r * (cosN * cosvw - sinN * sinvw * cosi);
            double centred_y = r * (sinN * cosvw + cosN * sinvw * cosi);
            double centred_z = r * (sinvw * Math.Sin(i));

            float rounded_x = Convert.ToSingle(centred_x);
            float rounded_y = Convert.ToSingle(centred_y);
            float rounded_z = Convert.ToSingle(centred_z);

            return new Vector3(rounded_x, rounded_y, rounded_z);
        }

        /// <summary>
        /// Works in ecliptic coordinates.
        /// Returns 
        /// </summary>
        /// <param name="current_time"></param>
        /// <returns>Longitude & latitude are in degrees, distance is in AU.</returns>
        public Vector3 current_longlatdist(DateTime current_time)
        {
            Vector3 xyz = current_location(current_time);

            float longitude = Convert.ToSingle(normalise_radians(Math.Atan2(xyz.y, xyz.x)) / DEG_TO_RAD);
            Vector2 xy = new Vector2(xyz.x, xyz.y);
            float latitude = Convert.ToSingle(normalise_radians(Math.Atan2(xyz.z, xy.magnitude)) / DEG_TO_RAD);

            float r = Convert.ToSingle(xyz.magnitude / AU);

            return new Vector3(longitude, latitude, r);
        }

        public static OrbitingBodyMathematics generate_planet (ORBITING_BODY planet)
        {
            switch (planet)
            {
                case ORBITING_BODY.SUN:
                    throw new Exception("Tried to generate the Sun as an OrbitingBody");
                case ORBITING_BODY.MERCURY:
                    return generate_mercury();
                case ORBITING_BODY.VENUS:
                    return generate_venus();
                case ORBITING_BODY.EARTH:
                    return generate_earth();
                case ORBITING_BODY.MOON:
                    return generate_moon(generate_earth());
                case ORBITING_BODY.MARS:
                    return generate_mars();
                case ORBITING_BODY.JUPITER:
                    return generate_jupiter();
                case ORBITING_BODY.SATURN:
                    return generate_saturn();
                case ORBITING_BODY.URANUS:
                    return generate_uranus();
                case ORBITING_BODY.NEPTUNE:
                    return generate_neptune();
                default:
                    // cannot happen - this is an enum!
                    throw new Exception("Dropped out of generate_planet enum - shouldn't ever happen.");
            }

        }

        public static OrbitingBodyMathematics generate_mercury()
        {
            // from http://www.astropixels.com/ephemeris/astrocal/astrocal2016gmt.html
            var last_mercury_perihelion = new DateTime(2016, 1, 8, 18, 0, 0);
            // from http://nssdc.gsfc.nasa.gov/planetary/factsheet/mercuryfact.html
            return new OrbitingBodyMathematics(57.91, 0.20563069, last_mercury_perihelion, 7.00487, 48.33167, 77.45645, 0.3301);
        }

        public static OrbitingBodyMathematics generate_venus()
        {
            // from http://www.astropixels.com/ephemeris/astrocal/astrocal2015gmt.html
            var last_venus_perihelion = new DateTime(2015, 11, 29, 6, 0, 0);
            // from http://nssdc.gsfc.nasa.gov/planetary/factsheet/venusfact.html
            return new OrbitingBodyMathematics(108.21, 0.00677323, last_venus_perihelion, 3.39471, 76.68069, 131.53298, 4.8676);
        }

        public static OrbitingBodyMathematics generate_earth()
        {
            // from http://aa.usno.navy.mil/data/docs/EarthSeasons.php
            var last_earth_perihelion = new DateTime(2016, 1, 2, 22, 49, 0);
            // from http://nssdc.gsfc.nasa.gov/planetary/factsheet/earthfact.html
            return new OrbitingBodyMathematics(149.60, 0.01671022, last_earth_perihelion, 0.00005, -11.26064, 102.94719, 5.9726);
        }

        public static OrbitingBodyMathematics generate_moon(OrbitingBodyMathematics earth)
        {
            // from https://www.fourmilab.ch/earthview/pacalc.html
            var last_perigee = new DateTime(2016, 2, 11, 14, 43, 0);

            // from http://nssdc.gsfc.nasa.gov/planetary/factsheet/earthfact.html
            return new OrbitingBodyMathematics(0.3844, 0.0549, last_perigee, 5.145, 125.08, 83.23, 0.07342, earth, earth.mass);
        }

        public static OrbitingBodyMathematics generate_mars()
        {
            // from http://www.astropixels.com/ephemeris/astrocal/astrocal2014gmt.html
            var last_mars_perihelion = new DateTime(2014, 12, 12, 12, 0, 0);
            // from http://nssdc.gsfc.nasa.gov/planetary/factsheet/marsfact.html
            return new OrbitingBodyMathematics(227.92, 0.09341233, last_mars_perihelion, 1.85061, 49.57854, 336.04084, 0.64174);
        }

        public static OrbitingBodyMathematics generate_jupiter()
        {
            // from http://www.astropixels.com/ephemeris/astrocal/astrocal2011gmt.html
            var last_jupiter_perihelion = new DateTime(2011, 3, 16, 11, 0, 0);
            // from http://nssdc.gsfc.nasa.gov/planetary/factsheet/jupiterfact.html
            return new OrbitingBodyMathematics(778.57, 0.04839266, last_jupiter_perihelion, 1.30530, 100.55615, 14.75385, 1898.3);
        }

        public static OrbitingBodyMathematics generate_saturn()
        {
            // from https://www.princeton.edu/~willman/planetary_systems/Sol/Saturn/
            // source seem to disagree
            var last_saturn_perihelion = new DateTime(2003, 7, 11, 13, 30, 0);
            // from http://nssdc.gsfc.nasa.gov/planetary/factsheet/saturnfact.html
            return new OrbitingBodyMathematics(1433.53, 0.05415060, last_saturn_perihelion, 2.48446, 113.71504, 92.43194, 568.36);
        }

        public static OrbitingBodyMathematics generate_uranus()
        {
            // from https://www.princeton.edu/~willman/planetary_systems/Sol/Uranus/
            var last_uranus_perihelion = new DateTime(1966, 9, 9);
            // from http://nssdc.gsfc.nasa.gov/planetary/factsheet/uranusfact.html
            return new OrbitingBodyMathematics(2872.46, 0.04716771, last_uranus_perihelion, 0.76986, 74.22988, 170.96424, 86.816);
        }

        public static OrbitingBodyMathematics generate_neptune()
        {
            // from https://www.princeton.edu/~willman/planetary_systems/Sol/Neptune/
            var last_uranus_perihelion = new DateTime(2046, 11, 13);
            // from http://nssdc.gsfc.nasa.gov/planetary/factsheet/neptunefact.html
            return new OrbitingBodyMathematics(4495.06, 0.00858587, last_uranus_perihelion, 1.76917, 131.72169, 44.97135, 102.42);
        }
    }
}
