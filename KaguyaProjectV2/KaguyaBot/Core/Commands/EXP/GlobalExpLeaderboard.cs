using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP
{
    public class GlobalExpLeaderboard : KaguyaBase
    {
        [ExpCommand]
        [Command("GlobalExpLeaderboard")]
        [Alias("gexplb", "gxplb")]
        [Summary("Displays the top 10 most active chatters globally as recorded by Kaguya's EXP system.")]
        [Remarks("")]
        [RequireContext(ContextType.Guild)]
        public async Task Command()
        {
            var top50 = (await DatabaseQueries.GetLimitAsync<User>(50, x => x.Experience > 0,
                x => x.Experience, true)).Where(x => !x.IsBlacklisted).ToList();

            int i = 1;

            var embed = new KaguyaEmbedBuilder
            {
                Fields = new List<EmbedFieldBuilder>()
            };

            foreach (var user in top50)
            {
                if (i > 10)
                    break;

                var socketUser = ConfigProperties.Client.GetUser(user.UserId);

                embed.Fields.Add(new EmbedFieldBuilder
                {
                    IsInline = false,
                    Name = $"{i}. {socketUser?.ToString() ?? $"[Unknown: {user.UserId}]"}",
                    Value = $"Level: {user.GlobalLevel():N0} ({user.PercentToNextLevel() * 100:N0}% {Centvrio.Emoji.Arrow.Right}" +
                            $" Lvl {user.GlobalLevel() + 1:N0}) " +
                            $"- Exp: {user.Experience:N0}"
                });

                i++;
            }

            await SendEmbedAsync(embed);
        }
    }
}
