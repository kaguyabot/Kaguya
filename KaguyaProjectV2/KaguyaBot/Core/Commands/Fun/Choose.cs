using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Fun
{
    public class Choose : KaguyaBase
    {
        [FunCommand]
        [Command("Choose")]
        [Summary("Kaguya will use her divine senses to pick the most suitable option from the choices provided!")]
        [Remarks("<item> <item> [...]\neggs bacon toast")]
        public async Task Command(params string[] options)
        {
            if (options.Length < 2)
            {
                await SendBasicErrorEmbedAsync("Please provide at least two options to choose from.\n" +
                                               "Example: `eggs toast` results in `eggs` or `toast` being selected.");
                return;
            }

            Random r = new Random();
            int index = r.Next(options.Length);

            var embed = new KaguyaEmbedBuilder(EmbedColor.GREEN)
            {
                Title = "Random Selection",
                Description = $"{Context.User.Mention} I choose `{options[index]}`!"
            };
            await SendEmbedAsync(embed);
        }
    }
}