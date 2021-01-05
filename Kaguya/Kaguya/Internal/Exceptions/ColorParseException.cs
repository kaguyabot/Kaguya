using System;

namespace Kaguya.Internal.Exceptions
{
    public class ColorParseException : Exception
    {
        public ColorParseException(string message) : base(message) { }
    }
}