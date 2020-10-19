using KaguyaProjectV2.KaguyaBot.Core.Global;
using System;

namespace KaguyaProjectV2.KaguyaBot.Core.Exceptions
{
    internal class KaguyaSupportException : Exception
    {
        /// <summary>
        /// Throws a new <see cref="KaguyaSupportException"/>, displaying a message.
        /// At the end of the message, the Kaguya Support Discord Server is automatically linked
        /// so that the user may find additional support.
        /// </summary>
        public KaguyaSupportException(string message) : base(KaguyaSupportExceptionMessage(message)) { }

        private static string KaguyaSupportExceptionMessage(string msg) => msg;
    }

    internal class KaguyaPremiumException : Exception
    {
        public KaguyaPremiumException(string message = null) : base(KaguyaPremiumExceptionMessage(message)) { }
        private static string KaguyaPremiumExceptionMessage(string msg = null) => $"\nSorry, only servers with an active [Kaguya Premium]({ConfigProperties.KAGUYA_STORE_URL}) subscription are allowed to use this feature.\n\n{msg}";
    }

    internal class KaguyaVoteException : Exception
    {
        public KaguyaVoteException(string message = null) : base(KaguyaVoteExceptionMessage(message)) { }

        private static string KaguyaVoteExceptionMessage(string msg = null) => $"\nSorry, you need to [vote on top.gg](https://top.gg/bot/538910393918160916) in order " +
                                                                               $"to use this feature. After voting, you will have access to this feature for 12 hours.";
    }
}