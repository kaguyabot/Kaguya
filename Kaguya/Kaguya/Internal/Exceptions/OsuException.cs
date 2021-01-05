using System;

namespace Kaguya.Internal.Exceptions
{
    public class OsuException : Exception
    {
        public OsuException(string msg) : base(msg) { }
    }
}