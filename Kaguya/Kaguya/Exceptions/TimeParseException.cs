using System;

namespace Kaguya.Exceptions
{
    public class TimeParseException : Exception
    {
        public TimeParseException(string userInputTime) : base($"{userInputTime} is an invalid time.\n" +
                                                               $"Times are formatted in `xdxhxmxs` where `x` is a number " +
                                                               $"and `dhms` represent `days`, `hours`, `minutes`, and `seconds` respectively.\n" +
                                                               $"Example: `2h30m` = 2 hours and 30 minutes. 5d5s = 5 days and 5 seconds.") { }
    }
}