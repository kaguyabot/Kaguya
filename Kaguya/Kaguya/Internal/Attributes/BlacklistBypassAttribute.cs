using System;

namespace Kaguya.Internal.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	internal class BlacklistBypassAttribute : Attribute {}
}