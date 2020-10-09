using System;

namespace KaguyaProjectV2.KaguyaBot.Core.Exceptions
{
    public class PokerStraightException : Exception
    {
        public PokerStraightException(string message) : base(PokerStraightExceptionMessage(message))
        { }

        private static string PokerStraightExceptionMessage(string msg) => msg;
       
    }
}