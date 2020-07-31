using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;

namespace KaguyaProjectV2.KaguyaBot.Core.Services
{
    public class NewOwnerNotificationService
    {
        public static async Task Trigger(SocketGuild newGuild)
        {
            var owner = newGuild.Owner;
            var embed = new KaguyaEmbedBuilder
            {
                Title = $"Salutations!",
                Description = $"Greetings, {owner.Username}. Thanks for adding me to your server! " +
                              $"Here's a few tips on how to get started:\n\n" +
                              $"- Kaguya's default prefix is `$`. Use this to invoke all commands. " +
                              $"All commands may be found by typing `$help`.\n" +
                              $"- Change my prefix to something else using `$prefix <new prefix>`. " +
                              $"Example: `$prefix k!`.\n" +
                              $"- If you forget the prefix, tag me at anytime to use commands.\n\n" +
                              $"If you need assistance, [click here to join my support server.](https://discord.gg/aumCJhr)\n\n" +
                              $"Enjoy!"
            };

            await owner.SendMessageAsync(embed: embed.Build());
        }
    }
}
