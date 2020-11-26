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
		Utility
	}

	[AttributeUsage(AttributeTargets.Class)]
	internal class ModuleAttribute : Attribute
	{
		public CommandModule Module { get; private set; }
		public ModuleAttribute(CommandModule module) => Module = module;
	}
}