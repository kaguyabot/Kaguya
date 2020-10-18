using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

#pragma warning disable 1998

namespace KaguyaProjectV2.KaguyaBot.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class AdminCommandAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class CurrencyCommandAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class ExpCommandAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class FunCommandAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class ReferenceCommandAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class MusicCommandAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class NsfwCommandAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Channel is SocketTextChannel guildCh)
            {
                if (guildCh.IsNsfw)
                    return Task.FromResult(PreconditionResult.FromSuccess());
            }
            return Task.FromResult(PreconditionResult.FromError("This command may only be invoked from NSFW-marked channels."));
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class OsuCommandAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class PremiumServerCommandAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var server = DatabaseQueries.GetOrCreateServerAsync(context.Guild.Id).Result;
            var user = DatabaseQueries.GetOrCreateUserAsync(context.User.Id).Result;
            return Task.FromResult(server.IsPremium || user.IsBotOwner || user.IsPremium
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError($"Sorry, but this server must be of [Kaguya Premium]({ConfigProperties.KaguyaStoreURL}) " +
                                               "status in order to use this command. This command may be used anywhere by " +
                                               "the key redeemer."));
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    internal class PremiumUserCommandAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var user = DatabaseQueries.GetOrCreateUserAsync(context.User.Id).Result;
            return Task.FromResult(user.IsBotOwner || user.IsPremium
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("This command may only be executed by the redeemer of a " +
                                               $"[Kaguya Premium]({ConfigProperties.KaguyaStoreURL}) key."));
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class UtilityCommandAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    internal class OwnerCommandAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Client.TokenType == TokenType.Bot)
                return (long)context.User.Id == (long)(await context.Client.GetApplicationInfoAsync()).Owner.Id ?
                    PreconditionResult.FromSuccess() : PreconditionResult.FromError("Command can only be run by the owner of the bot.");
            return PreconditionResult.FromError("RequireOwnerAttribute is not supported by this TokenType.");
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    internal class DisabledCommandAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(context.Guild.Id);

#if !DEBUG
            // Disable command for everyone except the bot owner.
            if(context.User.Id == ConfigProperties.BotConfig.BotOwnerId)
                return PreconditionResult.FromSuccess();
# endif

            await ConsoleLogger.LogAsync($"Disabled command attempted to be executed: " +
                                         $"[Command: {command.Name} | Guild: {context.Guild.Id} | User: {context.User.Id}]",LogLvl.INFO);
            return PreconditionResult.FromError("This command has been disabled. Please visit our support Discord server " +
                                                $"({server.CommandPrefix}invite) for more information. We apologize for " +
                                                $"any inconvenience this may present you, the developer is working dilligently " +
                                                $"to resolve the issue.");
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class DangerousCommandAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (((SocketGuildUser)context.User).GuildPermissions.Administrator)
                return PreconditionResult.FromSuccess();
            return PreconditionResult.FromError("As this is a dangerous command, the user must be an Administrator to use this command.");
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class RequireVoteCommandAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var user = await DatabaseQueries.GetOrCreateUserAsync(context.User.Id);
            var server = await DatabaseQueries.GetOrCreateServerAsync(context.Guild.Id);
            if (await user.HasRecentlyVotedAsync() || user.IsPremium || user.IsBotOwner)
            {
                return PreconditionResult.FromSuccess();
            }
            return PreconditionResult.FromError($"You need to [vote online](https://top.gg/bot/538910393918160916/vote) " +
                                                $"in order to use this feature. Voting will grant access to this feature " +
                                                $"until you are able to vote again.\n\n" +
                                                $"`{server.CommandPrefix}premium` members automatically bypass vote checks.");
        }
    }
}
