using System;

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
		Configuration
	}

	public enum ModuleRestriction
	{
		PremiumOnly
	}

	[AttributeUsage(AttributeTargets.Class)]
	internal class ModuleAttribute : Attribute
	{
		public CommandModule Module { get; }
		public ModuleAttribute(CommandModule module) => Module = module;
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	internal class RestrictionAttribute : Attribute
	{
		public ModuleRestriction Restriction { get; }
		public RestrictionAttribute(ModuleRestriction restriction) => Restriction = restriction;
	}
}