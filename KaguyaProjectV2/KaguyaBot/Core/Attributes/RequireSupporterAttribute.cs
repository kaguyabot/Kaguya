using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using System;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Attributes
{
    public class RequireSupporterAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var user = Users.GetUser(context.User.Id);

            if (user.IsSupporter)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError("Sorry, but you must be a supporter to use this command."));
        }
    }
}
