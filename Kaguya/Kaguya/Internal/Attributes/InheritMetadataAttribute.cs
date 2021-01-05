using System;
using Kaguya.Internal.Enums;

namespace Kaguya.Internal.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	internal class InheritMetadataAttribute : Attribute
	{
		public CommandMetadata Metadata { get; }
		public InheritMetadataAttribute(CommandMetadata metadata) { Metadata = metadata; }
	}
}