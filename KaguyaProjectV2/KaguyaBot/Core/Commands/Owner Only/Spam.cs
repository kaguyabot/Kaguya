using System;
using System.Threading.Tasks;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class Spam : KaguyaBase
    {
        [OwnerCommand]
        [Command("Spam")]
        [Summary("Generates some spam text. 5 messages by default.")]
        [Remarks("[number of messages, up to 20]")]
        public async Task Command(int num = 5)
        {
            if (num > 20)
            {
                await SendBasicErrorEmbedAsync("You cannot request a spam of more than 20 messages.");

                return;
            }

            var r = new Random();
            for (int i = 0; i < num; i++)
            {
                int numMsg = r.Next();
                await ReplyAsync($"{Context.User.Mention} spam: " + numMsg);
            }
        }
    }
}