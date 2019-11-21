using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    class AdminCommandAttribute : Attribute
    {

    }
    class CurrencyCommandAttribute : Attribute
    {

    }
    class ExpCommandAttribute : Attribute
    {

    }
    class FunCommandAttribute : Attribute
    {

    }
    class HelpCommandAttribute : Attribute
    {

    }
    class MusicCommandAttribute : Attribute
    {

    }
    class NsfwCommandAttribute : Attribute
    {

    }
    class OsuCommandAttribute : Attribute
    {

    }
    class SupporterCommandAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var user = Users.GetUser(context.User.Id);

            if (user.IsSupporter)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError("Sorry, but you must be a supporter to use this command."));
        }
    }
    class UtilityCommandAttribute : Attribute
    {

    }
}
