using System;

namespace Kaguya.Internal.Services.Models
{
    public class AntiraidData
    {
        public ulong ServerId { get; init; }
        public ulong UserId { get; init; }
        public string Action { get; init; }
        public DateTimeOffset JoinTime { get; init; }
        
        public static string FormattedAntiraidPunishment(string punishmentStr) => punishmentStr switch
        {
            "kick" => "kicked",
            "ban" => "banned",
            "mute" => "muted",
            "shadowban" => "shadowbanned",
            _ => punishmentStr
        };
    }
}