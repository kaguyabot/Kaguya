using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Fun
{
    public class Kiss : ModuleBase<ShardedCommandContext>
    {
        [FunCommand]
        [Command("Kiss")]
        [Summary("Kiss somebody, or multiple people!")]
        [Remarks("<user> {...}")]
        public async Task Command(params SocketGuildUser[] users)
        {
            var kissGif = await ConfigProperties.NekoClient.Action_v3.KissGif();

            if (users.Length == 1)
            {
                var embed = new KaguyaEmbedBuilder
                {
                    Title = $"Kiss | {new Emoji("💙")}",
                    Description = $"{Context.User.Mention} kissed {users[0].Mention}!",
                    ImageUrl = kissGif.ImageUrl
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
                    Title = $"Kiss | {new Emoji("💙")}",
                    Description = $"{Context.User.Mention} kissed {names.Humanize()}!",
                    ImageUrl = kissGif.ImageUrl
                };

                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}