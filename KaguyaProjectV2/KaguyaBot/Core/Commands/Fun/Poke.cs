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
    public class Poke : KaguyaBase
    {
        [FunCommand]
        [Command("Poke")]
        [Summary("Poke somebody, or multiple people!")]
        [Remarks("<user> {...}")]
        public async Task Command(params SocketGuildUser[] users)
        {
            Request pokeGif = await ConfigProperties.NekoClient.Action_v3.PokeGif();

            if (users.Length == 1)
            {
                var embed = new KaguyaEmbedBuilder
                {
                    Title = $"Poke!",
                    Description = $"{Context.User.Mention} poked {users[0].Mention}!",
                    ImageUrl = pokeGif.ImageUrl
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
                    Title = $"Poke!",
                    Description = $"{Context.User.Mention} poked {names.Humanize()}!",
                    ImageUrl = pokeGif.ImageUrl
                };

                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}