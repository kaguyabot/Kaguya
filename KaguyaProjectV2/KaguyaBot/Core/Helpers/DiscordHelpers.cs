using System;
using System.Linq;
using System.Reflection;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.TypeReaders;

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
    }
}