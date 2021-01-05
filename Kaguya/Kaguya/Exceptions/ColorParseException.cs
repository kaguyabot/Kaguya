using System;

namespace Kaguya.Exceptions
{
    public class ColorParseException : Exception
    {
        public ColorParseException(string message) : base(message) { }
    }
}