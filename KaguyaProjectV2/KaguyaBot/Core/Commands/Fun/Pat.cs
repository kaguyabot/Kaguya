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
    public class Pat : ModuleBase<ShardedCommandContext>
    {
        [FunCommand]
        [Command("Pat")]
        [Summary("Pat somebody, or multiple people!")]
        [Remarks("<user> {...}")]
        public async Task Command(params SocketGuildUser[] users)
        {
            var patGif = await ConfigProperties.NekoClient.Action_v3.PatGif();

            if (users.Length == 1)
            {
                var embed = new KaguyaEmbedBuilder
                {
                    Title = $"Pat!",
                    Description = $"{Context.User.Mention} patted {users[0].Mention}!",
                    ImageUrl = patGif.ImageUrl
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
                    Title = $"Pat!",
                    Description = $"{Context.User.Mention} patted {names.Humanize()}!",
                    ImageUrl = patGif.ImageUrl
                };

                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}