using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core
{
    /// <summary>
    ///     A static class containing all internally created events.
    /// </summary>
    public static class KaguyaEvents
    {
        /// <summary>
        ///     Fired when a guild's antiraid service is triggered.
        /// </summary>
        public static event Func<AntiRaidEventArgs, Task> OnRaid;
        public static void TriggerRaid(AntiRaidEventArgs arArgs) => OnRaid?.Invoke(arArgs);

        /// <summary>
        ///     Fired whenever a filtered phrase is detected in a guild.
        /// </summary>
        public static event Func<FilteredPhraseEventArgs, Task> OnFilteredPhrase;
        public static void TriggerFilteredPhrase(FilteredPhraseEventArgs fpArgs) => OnFilteredPhrase?.Invoke(fpArgs);

        /// <summary>
        ///     Fired whenever a warning is issued in a guild via the 'warn' command.
        /// </summary>
        public static event Func<WarnHandlerEventArgs, Task> OnWarn;
        public static void TriggerWarning(WarnHandlerEventArgs e) => OnWarn?.Invoke(e);

        /// <summary>
        ///     Fired whenever a warning is removed from a guild via the 'unwarn' command.
        /// </summary>
        public static event Func<UnwarnEventArgs, Task> OnUnwarn;
        public static void TriggerUnwarn(UnwarnEventArgs uwArgs) => OnUnwarn?.Invoke(uwArgs);

        /// <summary>
        ///     Fired whenever a user successfully catches a fish via the 'fish' command.
        /// </summary>
        public static event Func<FishHandlerEventArgs, Task> OnFish;
        public static async Task TriggerFish(FishHandlerEventArgs e) => OnFish?.Invoke(e);
    }

    public class AntiRaidEventArgs : EventArgs
    {
        public readonly IEnumerable<SocketGuildUser> GuildUsers;
        public readonly SocketGuild SocketGuild;
        public readonly string Punishment;
        
        public AntiRaidEventArgs(IEnumerable<SocketGuildUser> users, SocketGuild guild, string punishment)
        {
            GuildUsers = users;
            SocketGuild = guild;
            Punishment = punishment;
        }
    }

    public class FilteredPhraseEventArgs : EventArgs
    {
        public readonly IUser Author;
        public readonly IMessage Message;
        public readonly string Phrase;
        public readonly Server Server;

        public FilteredPhraseEventArgs(Server server, string phrase, IMessage message)
        {
            Server = server;
            Phrase = phrase;
            Message = message;
            Author = message.Author;
        }
    }

    public class WarnHandlerEventArgs : EventArgs
    {
        public WarnHandlerEventArgs(Server server, WarnedUser warnedUser)
        {
            Server = server;
            WarnedUser = warnedUser;
        }

        public Server Server { get; }
        public WarnedUser WarnedUser { get; }
    }

    public class UnwarnEventArgs
    {
        public Server Server { get; set; }
        public IGuildUser WarnedUser { get; set; }
        public IGuildUser ModeratorUser { get; set; }
        public string Reason { get; set; }
    }

    public class FishHandlerEventArgs : EventArgs
    {
        public FishHandlerEventArgs(User user, Fish fish, ICommandContext context)
        {
#pragma warning disable 4014
            ConsoleLogger.LogAsync($"User {user.UserId} has caught fish {fish} with value {fish.Value}.", LogLvl.DEBUG);
#pragma warning restore 4014

            User = user;
            Fish = fish;
            Context = context;
        }

        public User User { get; }
        public Fish Fish { get; }
        public ICommandContext Context { get; }
    }
}