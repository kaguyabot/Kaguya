using System;

namespace Kaguya.Internal.Enums
{
	[Flags]
	public enum ModuleRestriction : short
	{
		PremiumUser = 1,
		PremiumServer = 2,
		OwnerOnly = 4
	}
}