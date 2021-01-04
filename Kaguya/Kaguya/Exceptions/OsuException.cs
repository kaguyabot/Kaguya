using System;

namespace Kaguya.Exceptions
{
    public class OsuException : Exception
    {
        public OsuException(string msg) : base(msg) { }
    }
}