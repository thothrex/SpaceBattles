using System;
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
    public class OrbitingBody
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
        // Primary orbital elements
        private double semi_major_axis; // 10^6km
        private double eccentricity;
        private DateTime time_last_at_periapsis;

        private double inclination; // degrees
        private double longitude_of_ascending_node; // degrees
        private double argument_of_periapsis; // degrees - calculated from longitude

        private double mass; // 10^24kg
        private double mass_in_solar_masses; // mass / SOLAR_MASS
        public readonly double SOLAR_MASS = 1988500.0; // 10^24kg
        private double longitude_of_periapsis;

        // Derived elements
        private double periapsis_distance; // 10^6km
        private double apoapsis_distance; // 10^6 km
        private double orbital_period; // in seconds (SI Unit time)
        private double mean_angular_motion; // in degrees/radians per unit time (e.g. degrees/second)

        public OrbitingBody(double semi_major_axis,
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

            this.periapsis_distance = semi_major_axis * (1 - eccentricity);
            this.apoapsis_distance = semi_major_axis * (1 + eccentricity);
            this.argument_of_periapsis = longitude_of_periapsis - longitude_of_ascending_node;
            this.mass_in_solar_masses = mass / SOLAR_MASS;
            this.orbital_period = 2 * Math.PI * Math.Sqrt(
                  Math.Pow(semi_major_axis * DISTANCE_SCALE_TO_METRES, 3.0)
                / (CONSTANT_OF_GRAVITATION * (SOLAR_MASS * MASS_SCALE_TO_KG + mass * MASS_SCALE_TO_KG))
            );
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
    }
}
