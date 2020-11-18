using System;
using System.Linq;
using System.Reflection;
using Discord;
using Discord.WebSocket;

namespace KaguyaProjectV2.KaguyaBot.Core.Helpers
{
    public static class DiscordHelpers
    {
        /// <summary>
        /// Used to create an instance of an object where the constructor cannot be invoked normally.
        /// An example of this is a <see cref="Discord.Emote"/> in an overridden method.
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateInstance<T>(params object[] args)
        {
            Type type = typeof(T);
            object? instance = type.Assembly.CreateInstance(
                type.FullName, false,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, args, null, null);

            return (T) instance;
        }

        public static bool TryParseUser(string text, out ulong id)
        {
            if (MentionUtils.TryParseUser(text, out ulong parsedId))
            {
                id = parsedId;
                return true;
            }

            id = 0;
            return false;
        }

        /// <summary>
        /// Parses a <see cref="string"/> to a <see cref="SocketGuildUser"/>. If no user can be parsed,
        /// this method returns a null <see cref="SocketGuildUser"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="guild"></param>
        /// <returns></returns>
        public static SocketGuildUser ParseGuildUser(string text, SocketGuild guild)
        {
            SocketGuildUser user = null;
            if (MentionUtils.TryParseUser(text, out ulong id))
            {
                user = guild.Users.FirstOrDefault(x => x.Id == id);
            }
            else
            {
                SocketGuildUser parsedRes = guild.Users.FirstOrDefault(x => x.Username.Equals(text, StringComparison.OrdinalIgnoreCase));
                if (parsedRes != null)
                    user = parsedRes;
                else
                {
                    
                    SocketGuildUser nickParse = guild.Users.FirstOrDefault(x => !String.IsNullOrWhiteSpace(x.Nickname) &&
                                                                                x.Nickname.Equals(text, StringComparison.OrdinalIgnoreCase));
                    if (nickParse != null)
                        user = nickParse;
                }
            }
            
            return user;
        }
    }
}