using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
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
        ///     Fired whenever a user successfully catches a fish via the 'fish' command.
        /// </summary>
        public static event Func<FishHandlerEventArgs, Task> OnFish;
        public static async Task TriggerFish(FishHandlerEventArgs e) => OnFish?.Invoke(e);
        
        /// <summary>
        ///     Fired whenever a warning is issued in a guild via the 'warn' command.
        /// </summary>
        public static event Func<IModeratorEventArgs, Task> OnWarn;
        public static void TriggerWarning(IModeratorEventArgs mArgs) => OnWarn?.Invoke(mArgs);

        /// <summary>
        ///     Fired whenever a warning is removed from a guild via the 'unwarn' command.
        /// </summary>
        public static event Func<IModeratorEventArgs, Task> OnUnwarn;
        public static void TriggerUnwarn(IModeratorEventArgs mArgs) => OnUnwarn?.Invoke(mArgs);

        /// <summary>
        /// Fired whenever a user is shadowbanned via the 'shadowban' command.
        /// </summary>
        public static event Func<IModeratorEventArgs, Task> OnShadowban;
        public static void TriggerShadowban(IModeratorEventArgs mArgs) => OnShadowban?.Invoke(mArgs);

        /// <summary>
        /// Fired whenever a user is unshadowbanned via the 'unshadowban' command.
        /// </summary>
        public static event Func<IModeratorEventArgs, Task> OnUnshadowban;
        public static void TriggerUnshadowban(IModeratorEventArgs mArgs) => OnUnshadowban?.Invoke(mArgs);

        /// <summary>
        /// Fired whenever a user is muted via the 'mute' command.
        /// </summary>
        public static event Func<IModeratorEventArgs, Task> OnMute;
        public static void TriggerMute(IModeratorEventArgs mArgs) => OnMute?.Invoke(mArgs);

        /// <summary>
        /// Fired whenever a user is unmuted via the 'unmute' command.
        /// </summary>
        public static event Func<IModeratorEventArgs, Task> OnUnmute;
        public static void TriggerUnmute(IModeratorEventArgs mArgs) => OnUnmute?.Invoke(mArgs);
    }

    public class AntiRaidEventArgs
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

    public class FilteredPhraseEventArgs
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

    public class FishHandlerEventArgs
    {
        public FishHandlerEventArgs(User user, Fish fish, ICommandContext context)
        {
            // todo: Remove logging. Log this somewhere else, definitely not in the constructor!
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

    public class ModeratorEventArgs : IModeratorEventArgs
    {
        public Server Server { get; init; }
        public SocketGuild Guild { get; init; }
        public SocketGuildUser ActionedUser { get; init; }
        public SocketGuildUser ModeratorUser { get; init; }
        public string Reason { get; init; }

        public ModeratorEventArgs(Server server, SocketGuild guild, SocketGuildUser actionedUser, 
            SocketGuildUser moderatorUser, string reason)
        {
            Server = server;
            Guild = guild;
            ActionedUser = actionedUser;
            ModeratorUser = moderatorUser;
            Reason = reason;
        }

        protected ModeratorEventArgs(IModeratorEventArgs a) : this(a.Server, a.Guild, a.ActionedUser, a.ModeratorUser, a.Reason) { }
    }

    public sealed class ModeratorEventLogger : ModeratorEventArgs
    {
        public string FormattedTimeStamp { get; }
        public InternalModerationAction Action { get; }
        public string PastTenseAction { get; }
        public string Emoji { get; }
        public ulong LogChannelId { get; }
        public ModeratorEventLogger(IModeratorEventArgs args, InternalModerationAction action, string emoji, 
            string formattedTimeStamp, ulong logChannelId) : base(args)
        {
            Action = action;
            Emoji = emoji;
            FormattedTimeStamp = formattedTimeStamp;
            LogChannelId = logChannelId;
            PastTenseAction = GetPastTenseAction();
        }

        public async Task LogModerationAction()
        {
            if (LogChannelId == 0)
                return;

            string gLog = GetGuildLogString();
            string exLog = GetExceptionLogString();
            SocketTextChannel c = Guild.GetTextChannel(LogChannelId);

            try
            {
                await c.SendMessageAsync(gLog);
            }
            catch (Exception)
            {
                await ConsoleLogger.LogAsync(exLog, LogLvl.WARN);
                ResetLogChannel();
                await DatabaseQueries.UpdateAsync(Server);
            }
        }
        
        /// <summary>
        /// Generates the log string that is sent to the <see cref="LogChannelId"/> in this <see cref="Guild"/>
        /// </summary>
        /// <returns></returns>
        public string GetGuildLogString()
        {
            var sb = new StringBuilder($"{Emoji} `[{FormattedTimeStamp}]` `ID: {ActionedUser.Id}` ");
            sb.Append($"**{ActionedUser}** was {PastTenseAction} by {ModeratorUser}. Reason: {Reason}");

            return sb.ToString();
        }

        public string GetExceptionLogString()
        {
            var sb = new StringBuilder();
            sb.Append($"Failed to send {Action.Humanize(LetterCasing.LowerCase)} log to channel {LogChannelId} ");
            sb.Append($"in guild {Guild.Id}. Resetting this log channel to 0 so it ");
            sb.Append("doesn't happen again!");

            return sb.ToString();
        }
        
        private string GetPastTenseAction() => Action switch // todo: Make a method that makes things past tense?
        {
            InternalModerationAction.MUTE => "muted",
            InternalModerationAction.UNMUTE => "unmuted",
            InternalModerationAction.WARN => "warned",
            InternalModerationAction.UNWARN => "unwarned",
            InternalModerationAction.SHADOWBAN => "shadowbanned",
            InternalModerationAction.UNSHADOWBAN => "unshadowbanned",
            _ => default
        };

        private void ResetLogChannel()
        {
            switch (Action)
            {
                case InternalModerationAction.MUTE:
                    Server.LogMutes = 0;

                    break;
                case InternalModerationAction.UNMUTE:
                    Server.LogUnmutes = 0;

                    break;
                case InternalModerationAction.WARN:
                    Server.LogWarns = 0;

                    break;
                case InternalModerationAction.UNWARN:
                    Server.LogUnwarns = 0;

                    break;
                case InternalModerationAction.SHADOWBAN:
                    Server.LogShadowbans = 0;

                    break;
                case InternalModerationAction.UNSHADOWBAN:
                    Server.LogUnshadowbans = 0;

                    break;
            }
        }
    }

    public enum InternalModerationAction
    {
        WARN,
        UNWARN,
        MUTE,
        UNMUTE,
        SHADOWBAN,
        UNSHADOWBAN
    }
}