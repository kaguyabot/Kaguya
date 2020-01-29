using System;
using Discord.Commands;
using System.Threading.Tasks;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP.Profile
{
    public class Profile : KaguyaBase
    {
        [ExpCommand]
        [Command("Profile")]
        [Alias("p")]
        [Summary("Displays your Kaguya Profile, showing off how much EXP you have earned, as well " +
                 "as some other stats!")]
        [Remarks("")]
        [RequireContext(ContextType.Guild)]
        public async Task Command()
        {
            var p = new ProfileImage();
            var image = await p.GenerateImageStream();
            await Context.Channel.SendFileAsync(image, $"Kaguya_Profile_" +
                                                       $"{Context.User.Username}_{DateTime.Now.Month}_" +
                                                       $"{DateTime.Now.Day}_{DateTime.Now.Year}.png");
        }
    }
}
