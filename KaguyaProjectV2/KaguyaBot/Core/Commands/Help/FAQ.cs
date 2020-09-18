using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class FAQ : KaguyaBase
    {
        [ReferenceCommand]
        [Command("Faq")]
        [Summary("Displays a link to the Kaguya FAQ.")]
        [Remarks("")]
        public async Task Command()
        {
            string faq = "https://docs.google.com/document/d/1AWYatYUk3qiDN3OykZhKkQ_1ThVtpilXSjohHeSxE0E/edit?usp=sharing";
            await SendBasicSuccessEmbedAsync($"{Context.User.Mention} [[Kaguya FAQ]]({faq})");
        }
    }
}
