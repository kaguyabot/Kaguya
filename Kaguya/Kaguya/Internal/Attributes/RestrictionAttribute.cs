using System;
using System.Threading.Tasks;
using Discord.Commands;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Enums;
using Kaguya.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kaguya.Internal.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    internal class RestrictionAttribute : PreconditionAttribute
    {
        public ModuleRestriction Restriction { get; }
        public RestrictionAttribute(ModuleRestriction restriction)
        {
            Restriction = restriction;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var kaguyaUserRepository = services.GetRequiredService<KaguyaUserRepository>();
            var adminConfigurations = services.GetRequiredService<IOptions<AdminConfigurations>>();
			
            if ((Restriction & ModuleRestriction.OwnerOnly) != 0)
            {
                if (context.User.Id != adminConfigurations.Value.OwnerId)
                {
                    return PreconditionResult.FromError("You must be the owner of the bot to execute this command.");
                }
            }

            if ((Restriction & ModuleRestriction.PremiumOnly) != 0)
            {
                var user = await kaguyaUserRepository.GetOrCreateAsync(context.User.Id);
				
                return user.IsPremium 
                    ? PreconditionResult.FromSuccess() 
                    : PreconditionResult.FromError($"You must purchase a [Kaguya Premium]({Global.StoreUrl}) subscription to use this command.");
            }
			
            return PreconditionResult.FromSuccess();
        }
    }
}