/// Courtesy of StackOverflow Pascal
/// http://stackoverflow.com/questions/13645149/what-is-the-correct-exception-to-throw-for-unhandled-enum-values

using System;

namespace SpaceBattles
{
    public class UnexpectedEnumValueException<T> : Exception
    {
        public UnexpectedEnumValueException(T value)
            : base("Value " + value + " of enum " + typeof(T).Name + " is not supported")
        {
        }
    }
}
