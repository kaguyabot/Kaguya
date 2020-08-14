using System;
using System.Threading.Tasks;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Fun
{
    public class CoinFlip : KaguyaBase
    {
        [FunCommand]
        [Command("CoinFlip")]
        [Alias("cf")]
        [Summary("Flips a two-sided coin, resulting in either heads or tails.")]
        [Remarks("")]
        public async Task Command()
        {
            var r = new Random();
            int flip = r.Next();

            bool heads = flip % 2 == 0;

            string emote = heads 
                ? "<:CoinHeads:743919521965801472>" 
                : "<:CoinTails:743919521936572456>";

            var embed = new KaguyaEmbedBuilder(heads ? EmbedColor.LIGHT_BLUE : EmbedColor.LIGHT_PURPLE)
            {
                Description = $"{emote} {Context.User.Mention} You rolled {(heads ? "Heads" : "Tails")}!"
            };

            await Context.Channel.SendEmbedAsync(embed);
        }
    }
}