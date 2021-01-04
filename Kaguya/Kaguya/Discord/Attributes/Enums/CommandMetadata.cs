using System;

namespace Kaguya.Discord.Attributes.Enums
{
    [Flags]
    public enum CommandMetadata
    {
        Summary = 1,
        Remarks = 2
    }
}