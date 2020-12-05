using System;
using System.Threading.Tasks;
using Discord.Commands;
using Kaguya.Database.Repositories;
using Kaguya.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kaguya.Discord.Attributes
{
	public enum CommandModule
	{
		Administration,
		Games,
		Exp,
		Emotion,
		Reference,
		Music,
		Nsfw,
		Osu,
		Utility,
		Configuration,
		OwnerOnly
	}

	[Flags]
	public enum ModuleRestriction : short
	{
		PremiumOnly = 1,
		OwnerOnly = 2
	}

	[AttributeUsage(AttributeTargets.Class)]
	internal class ModuleAttribute : Attribute
	{
		public CommandModule Module { get; }
		public ModuleAttribute(CommandModule module) => Module = module;
	}

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
					: PreconditionResult.FromError($"You must purchase a [Kaguya Premium]({Global.KaguyaStoreUrl}) subscription to use this command.");
			}
			
			return PreconditionResult.FromSuccess();
		} 
	}
}