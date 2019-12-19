using System;
using System.Collections.Generic;
using System.Text;

namespace KaguyaProjectV2.KaguyaBot.Core.Interfaces
{
    public interface IKey
    {
        string Key { get; set; }
        long LengthInSeconds { get; set; }
        ulong KeyCreatorId { get; set; }
        ulong UserId { get; set; }
        double Expiration { get; set; }
    }
}
