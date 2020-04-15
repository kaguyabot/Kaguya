using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;

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
    internal class HelpCommandAttribute : Attribute
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
    internal class PremiumCommandAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var server = DatabaseQueries.GetOrCreateServerAsync(context.Guild.Id).Result;
            var user = DatabaseQueries.GetOrCreateUserAsync(context.User.Id).Result;
            return Task.FromResult(server.IsPremium || user.IsBotOwner || user.IsPremiumAsync().Result
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("Sorry, but this server must be of premium status in order to use this command. " +
                                               "If the server isn't premium, this command may only be used by the holder " +
                                               "of a valid, unexpired Kaguya Premium key."));
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

    internal class DangerousCommandAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (((SocketGuildUser)context.User).GuildPermissions.Administrator)
                return PreconditionResult.FromSuccess();
            return PreconditionResult.FromError("As this is a dangerous command, the user must be an Administrator to use this command.");
        }
    }
}
