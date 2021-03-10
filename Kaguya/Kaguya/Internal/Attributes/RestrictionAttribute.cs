using Discord.Commands;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Enums;
using Kaguya.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Kaguya.Internal.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	internal class RestrictionAttribute : PreconditionAttribute
	{
		public RestrictionAttribute(ModuleRestriction restriction) { this.Restriction = restriction; }
		public ModuleRestriction Restriction { get; }

		public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			var kaguyaUserRepository = services.GetRequiredService<KaguyaUserRepository>();
			var kaguyaserverRepository = services.GetRequiredService<KaguyaServerRepository>();
			var adminConfigurations = services.GetRequiredService<IOptions<AdminConfigurations>>();

			if ((this.Restriction & ModuleRestriction.OwnerOnly) != 0)
			{
				if (context.User.Id != adminConfigurations.Value.OwnerId)
				{
					return PreconditionResult.FromError("You must be the owner of the bot to execute this command.");
				}
			}

			if ((this.Restriction & ModuleRestriction.PremiumUser) != 0)
			{
				var user = await kaguyaUserRepository.GetOrCreateAsync(context.User.Id);

				return user.IsPremium
					? PreconditionResult.FromSuccess()
					: PreconditionResult.FromError(
						$"You must purchase a [Kaguya Premium]({Global.StoreUrl}) subscription to use this command.");
			}

			if ((this.Restriction & ModuleRestriction.PremiumServer) != 0)
			{
				var server = await kaguyaserverRepository.GetOrCreateAsync(context.Guild.Id);

				return server.IsPremium
					? PreconditionResult.FromSuccess()
					: PreconditionResult.FromError(
						$"You must purchase a [Kaguya Premium]({Global.StoreUrl}) subscription to use this command.");
			}

			return PreconditionResult.FromSuccess();
		}
	}
}