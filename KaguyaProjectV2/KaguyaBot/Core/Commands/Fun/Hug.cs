using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NekosSharp;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Fun
{
    public class Hug : KaguyaBase
    {
        [FunCommand]
        [Command("Hug")]
        [Summary("Hug somebody, or multiple people!")]
        [Remarks("<user> {...}")]
        public async Task Command(params SocketGuildUser[] users)
        {
            Request hugGif = await ConfigProperties.NekoClient.Action_v3.HugGif();

            if (users.Length == 1)
            {
                var embed = new KaguyaEmbedBuilder
                {
                    Title = "Hug <3",
                    Description = $"{Context.User.Mention} hugged {users[0].Mention}!",
                    ImageUrl = hugGif.ImageUrl
                };

                await ReplyAsync(embed: embed.Build());

                return;
            }
            else
            {
                var names = new List<string>();
                users.ToList().ForEach(x => names.Add(x.Mention));

                if (names.Count == 0)
                    names.Add("the air");

                var embed = new KaguyaEmbedBuilder
                {
                    Title = "Hug <3",
                    Description = $"{Context.User.Mention} hugged {names.Humanize()}!",
                    ImageUrl = hugGif.ImageUrl
                };

                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}