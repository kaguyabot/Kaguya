using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using System;
using System.Threading.Tasks;
using Discord;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class AdminCommandAttribute : Attribute
    {

    }
    internal class CurrencyCommandAttribute : Attribute
    {

    }
    internal class ExpCommandAttribute : Attribute
    {

    }
    internal class FunCommandAttribute : Attribute
    {

    }
    internal class HelpCommandAttribute : Attribute
    {

    }
    internal class MusicCommandAttribute : Attribute
    {

    }
    internal class NsfwCommandAttribute : Attribute
    {

    }
    internal class OsuCommandAttribute : Attribute
    {

    }

    internal class SupporterCommandAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var user = UserQueries.GetOrCreateUser(context.User.Id);

            return Task.FromResult(user.Result.IsSupporter ? PreconditionResult.FromSuccess() : 
                PreconditionResult.FromError("Sorry, but you must be a supporter to use this command."));
        }
    }

    internal class PremiumServerCommandAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var server = ServerQueries.GetOrCreateServer(context.Guild.Id);

            return Task.FromResult(server.Result.IsPremium ? PreconditionResult.FromSuccess() :
                PreconditionResult.FromError("Sorry, but this server must be of premium status in order to use this command."));
        }
    }

    internal class UtilityCommandAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    internal class OwnerCommandAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Client.TokenType == TokenType.Bot)
                return (long)context.User.Id == (long)(await context.Client.GetApplicationInfoAsync().ConfigureAwait(false)).Owner.Id ? 
                    PreconditionResult.FromSuccess() : PreconditionResult.FromError("Command can only be run by the owner of the bot.");
            return PreconditionResult.FromError("RequireOwnerAttribute is not supported by this TokenType.");
        }
    }
}
