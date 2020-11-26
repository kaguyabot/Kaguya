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

	[AttributeUsage(AttributeTargets.Class)]
	internal class ModuleAttribute : Attribute
	{
		public CommandModule Module { get; }
		public ModuleAttribute(CommandModule module) => Module = module;
	}
}