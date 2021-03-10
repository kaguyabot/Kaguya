using Kaguya.Internal.Enums;
using System;

namespace Kaguya.Internal.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
	internal class ModuleAttribute : Attribute
	{
		public ModuleAttribute(CommandModule module) { this.Module = module; }
		public CommandModule Module { get; }
	}
}