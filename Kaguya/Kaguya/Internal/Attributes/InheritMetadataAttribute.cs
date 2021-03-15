using Kaguya.Internal.Enums;
using System;

namespace Kaguya.Internal.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	internal class InheritMetadataAttribute : Attribute
	{
		public InheritMetadataAttribute(CommandMetadata metadata) { this.Metadata = metadata; }
		public CommandMetadata Metadata { get; }
	}
}