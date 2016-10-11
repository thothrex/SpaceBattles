using System;

namespace SpaceBattles
{

    /// <summary>
    /// No code contracts due to Unity using old .net
    /// so here are some poor man's versions
    /// </summary>
    public class MyContract
    {
        public static void RequireField(bool condition, string condition_description, string field_name)
        {
            if (!condition)
            {
                throw new InvalidOperationException(
                        field_name
                     + " does not satisfy the condition: "
                     + condition_description
                );
            }
        }

        public static void RequireArgument
            (bool condition, string condition_description, string arg_name)
        {
            if (!condition)
            {
                throw new ArgumentException(
                    "Argument does not satisfy condition: " + condition_description,
                    arg_name
                );
            }
        }

        public static void RequireFieldNotNull(object field, string field_name)
        {
            RequireField(field != null, "is not null", field_name);
        }

        public static void RequireArgumentNotNull
            (Object arg, string arg_name)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(arg_name);
            }
        }
    }

}