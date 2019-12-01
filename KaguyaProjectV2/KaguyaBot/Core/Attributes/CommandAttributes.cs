using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using System;
using System.Threading.Tasks;
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
            var user = UserQueries.GetUser(context.User.Id);

            return Task.FromResult(user.IsSupporter ? PreconditionResult.FromSuccess() : 
                PreconditionResult.FromError("Sorry, but you must be a supporter to use this command."));
        }
    }

    internal class UtilityCommandAttribute : Attribute
    {

    }
}
