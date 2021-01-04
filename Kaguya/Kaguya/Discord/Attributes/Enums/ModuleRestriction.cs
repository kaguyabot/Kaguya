using System;

namespace Kaguya.Discord.Attributes.Enums
{
    [Flags]
    public enum ModuleRestriction : short
    {
        PremiumOnly = 1,
        OwnerOnly = 2
    }
}