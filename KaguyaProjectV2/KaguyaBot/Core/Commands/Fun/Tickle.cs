using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Fun
{
    public class Tickle : KaguyaBase
    {
        [FunCommand]
        [Command("Tickle")]
        [Summary("Tickle somebody, or multiple people!")]
        [Remarks("<user> {...}")]
        public async Task Command(params SocketGuildUser[] users)
        {
            var tickleGif = await ConfigProperties.NekoClient.Action_v3.TickleGif();

            if (users.Length == 1)
            {
                var embed = new KaguyaEmbedBuilder
                {
                    Title = "Tickle!",
                    Description = $"{Context.User.Mention} tickled {users[0].Mention}!",
                    ImageUrl = tickleGif.ImageUrl
                };

                await ReplyAsync(embed: embed.Build());
                return;
            }
            else
            {
                var names = new List<string>();
                users.ToList().ForEach(x => names.Add(x.Mention));

                var embed = new KaguyaEmbedBuilder
                {
                    Title = "Tickle!",
                    Description = $"{Context.User.Mention} tickled {names.Humanize()}!",
                    ImageUrl = tickleGif.ImageUrl
                };

                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}