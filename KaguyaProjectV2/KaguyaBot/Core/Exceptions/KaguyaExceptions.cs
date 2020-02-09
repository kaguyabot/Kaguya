using KaguyaProjectV2.KaguyaBot.Core.Global;
using System;

namespace KaguyaProjectV2.KaguyaBot.Core.Exceptions
{
    class KaguyaSupportException : Exception
    {
        /// <summary>
        /// Throws a new <see cref="KaguyaSupportException"/>. This basically does nothing, please don't use this lol.
        /// Use the overloads instead :D
        /// </summary>
        public KaguyaSupportException()
        {
        }

        /// <summary>
        /// Throws a new <see cref="KaguyaSupportException"/>, displaying a message.
        /// At the end of the message, the Kaguya Support Discord Server is automatically linked
        /// so that the user may find additional support.
        /// </summary>
        public KaguyaSupportException(string message) : base(KaguyaSupportExceptionMessage(message))
        { }
        private static string KaguyaSupportExceptionMessage(string msg)
        {
            return msg + $"\n\nPlease join this server if additional assistance is needed: " +
                   $"\n[[Kaguya Support Discord Server]]({ConfigProperties.KaguyaSupportDiscordServer})\n" +
                   $"[[Report a bug]](https://github.com/stageosu/Kaguya/issues/new?assignees=&labels=Bug&template=bug-report.md&title=)";
        }

        /// <summary>
        /// Throws a new <see cref="KaguyaSupportException"/>, displaying a message with an inner exception.
        /// At the end of the message, the Kaguya Support Discord Server is automatically linked
        /// so that the user may find additional support.
        /// </summary>
        public KaguyaSupportException(string message, Exception inner) : base(message, inner)
        {
            message += $"\n\nPlease join this server for additional assistance: " +
                       $"[[Kaguya Support Discord Server]]({ConfigProperties.KaguyaSupportDiscordServer})";
        }
    }

    class KaguyaSupporterException : Exception
    {
        public KaguyaSupporterException(string message = null) : base(KaguyaSupporterExceptionMessage(message))
        {
        }

        private static string KaguyaSupporterExceptionMessage(string msg = null)
        {
            return $"\nSorry, only active [Kaguya Supporters]({ConfigProperties.KaguyaStore}) are allowed to use the described feature: \n\n`{msg}`";
        }
    }

    class KaguyaPremiumException : Exception
    {
        public KaguyaPremiumException(string message = null) : base(KaguyaPremiumExceptionMessage(message))
        {
        }

        private static string KaguyaPremiumExceptionMessage(string msg = null)
        {
            return $"\nSorry, only servers with an active [Kaguya Premium]({ConfigProperties.KaguyaStore}) subscription are allowed to use this feature.\n\n`{msg}`";
        }
    }
}
