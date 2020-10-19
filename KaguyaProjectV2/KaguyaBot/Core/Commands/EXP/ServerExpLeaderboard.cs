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
using Discord.WebSocket;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP
{
    public class ServerExpLeaderboard : KaguyaBase
    {
        [ExpCommand]
        [Command("ServerExpLeaderboard")]
        [Alias("sexplb", "sxplb", "xplb")]
        [Summary("Displays the top 10 most active chatters based on the server's exp leaderboard.")]
        [Remarks("")]
        [RequireContext(ContextType.Guild)]
        public async Task Command()
        {
            List<ServerExp> top50 = await DatabaseQueries.GetLimitAsync<ServerExp>(50,
                x => x.Exp > 0 && x.ServerId == Context.Guild.Id,
                x => x.Exp, true);

            int i = 1;

            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var embed = new KaguyaEmbedBuilder
            {
                Title = $"Exp Leaderboard for {Context.Guild}",
                Fields = new List<EmbedFieldBuilder>()
            };

            foreach (ServerExp element in top50)
            {
                if (i > 10)
                    break;

                SocketUser socketUser = Client.GetUser(element.UserId);
                User user = await DatabaseQueries.GetOrCreateUserAsync(element.UserId);

                embed.Fields.Add(new EmbedFieldBuilder
                {
                    IsInline = false,
                    Name = $"{i}. {socketUser?.ToString().Split('#').First() ?? $"[Unknown User: {user.UserId}]"}",
                    Value = $"Level: {user.ServerLevel(server):N0} ({user.PercentToNextServerLevel(server) * 100:N0}% {Centvrio.Emoji.Arrow.Right}" +
                            $" Lvl {user.ServerLevel(server) + 1:N0}) " +
                            $"- Exp: {element.Exp:N0}"
                });

                i++;
            }

            await SendEmbedAsync(embed);
        }
    }
}