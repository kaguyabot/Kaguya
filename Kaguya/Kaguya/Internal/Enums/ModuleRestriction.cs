using System;

namespace Kaguya.Internal.Enums
{
    [Flags]
    public enum ModuleRestriction : short
    {
        PremiumOnly = 1,
        OwnerOnly = 2
    }
}