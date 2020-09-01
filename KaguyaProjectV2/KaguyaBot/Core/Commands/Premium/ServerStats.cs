using Discord;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Premium
{
    public class ServerStats : KaguyaBase
    {
        [PremiumServerCommand]
        [AdminCommand]
        [Command("ServerStats")]
        [Alias("ss")]
        [Summary("Displays a detailed array of statistics about the server, including stats collected by Kaguya.")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Command()
        {
            var guild = Context.Guild;
            var server = await DatabaseQueries.GetOrCreateServerAsync(guild.Id);
            var fish = await DatabaseQueries.GetAllForServerAsync<Fish>(server.ServerId);
            var praise = await DatabaseQueries.GetAllForServerAsync<Praise>(server.ServerId);
            var warnedUsers = await DatabaseQueries.GetAllForServerAsync<WarnedUser>(server.ServerId);
            var commandHistory = await DatabaseQueries.GetAllForServerAsync<CommandHistory>(server.ServerId);
            var autoAssignedRoles = await DatabaseQueries.GetAllForServerAsync<AutoAssignedRole>(server.ServerId);
            var antiRaidConfig = await DatabaseQueries.GetFirstForServerAsync<AntiRaidConfig>(server.ServerId);
            var mutedUsers = await DatabaseQueries.GetAllForServerAsync<MutedUser>(server.ServerId);
            var premiumKeys = await DatabaseQueries.GetAllForServerAsync<PremiumKey>(server.ServerId);

            var percentageOnline =
                ((double)guild.Users.Count(x => x.Status != UserStatus.Offline && x.Status != UserStatus.Invisible) /
                 guild.MemberCount) * 100;

            double premiumExpiration = server.PremiumExpirationDate;

            var genSb = new StringBuilder();
            genSb.AppendLine($"Name: `{guild}`");
            genSb.AppendLine($"ID: `{guild.Id}`");
            genSb.AppendLine($"Date Created: `{guild.CreatedAt.Date.ToLongDateString()}`");
            genSb.AppendLine($"Owner: `{guild.Owner}`");
            genSb.AppendLine($"Total Channels: `{guild.TextChannels.Count + guild.VoiceChannels.Count:N0}`");
            genSb.AppendLine($"Voice Channels: `{guild.VoiceChannels.Count:N0}`");
            genSb.AppendLine($"Text Channels: `{guild.TextChannels.Count:N0}`");
            genSb.AppendLine($"Emotes: `{guild.Emotes.Count:N0}`");
            genSb.AppendLine($"Multi-factor Authentication: `{guild.MfaLevel.Humanize(LetterCasing.Sentence)}`");
            genSb.AppendLine($"Verification Level: `{guild.VerificationLevel}`");
            genSb.AppendLine($"Total Member Count: `{guild.MemberCount:N0}`");
            genSb.AppendLine($"Percentage of users online: `{percentageOnline:F}%`");
            genSb.AppendLine($"Voice-Channel AFK Timeout: `{(guild.AFKTimeout != 0 ? TimeSpan.FromSeconds(guild.AFKTimeout).Humanize() : "Disabled")}`");
            genSb.AppendLine($"AFK Voice Channel: `{(guild.AFKChannel is null ? "Disabled." : guild.AFKChannel.Name)}`");

            var embed = new KaguyaEmbedBuilder
            {
                Title = $"Kaguya Statistics for {guild.Name}",
                Fields = new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder
                    {
                        Name = "General Information",
                        Value = genSb.ToString()
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Kaguya Statistics",
                        Value = $"Premium Expiration: `{DateTime.FromOADate(DateTime.Now.ToOADate() + premiumExpiration).Humanize(false)}`\n" +
                                $"Total Praise Given: `{praise.Count:N0}`\n" +
                                $"Total Fish Caught: `{fish?.Count(x => x.FishType != FishType.BAIT_STOLEN).ToString("N0") ?? "No fish caught."}`\n" +
                                $"Rarest Fish Caught: `{fish?.OrderBy(x => x.FishType).First().FishType.ToString() ?? "No fish caught."}`\n" +
                                $"Total Active Warnings: `{warnedUsers.Count:N0}`\n" +
                                $"Currently Muted Users: `{mutedUsers.Count:N0}`\n" +
                                $"Total Kaguya Commands Used: `{commandHistory.Count:N0}`\n" +
                                $"Active Auto-Assigned Roles: `{autoAssignedRoles.Count:N0}`\n" +
                                $"Anti-Raid Status: `{(antiRaidConfig != null ? "Enabled." : "Disabled.")}`\n"
                    }
                }
            };
            await SendEmbedAsync(embed);
        }
    }
}