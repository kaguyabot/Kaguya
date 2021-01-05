using System;
using Kaguya.Discord.DiscordExtensions;

namespace Kaguya.Internal.Exceptions
{
    public class OsuUserNotFoundException : Exception
    {
        public OsuUserNotFoundException(string username) : base($"No osu! username or ID match was found for {username.AsBold()}.") { }
    }
}