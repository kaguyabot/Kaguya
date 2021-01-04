using System;
using System.Diagnostics.CodeAnalysis;
using Kaguya.Discord.Attributes.Enums;

namespace Kaguya.Discord.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	internal class InheritMetadataAttribute : Attribute
	{
		public CommandMetadata Metadata { get; }
		public InheritMetadataAttribute(CommandMetadata metadata) { Metadata = metadata; }
	}
}