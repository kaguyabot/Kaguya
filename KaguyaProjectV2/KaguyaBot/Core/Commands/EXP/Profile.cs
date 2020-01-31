using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Images;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP
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
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);

            var p = new ProfileImage();
            var image = await p.GenerateProfileImageStream(user, server, Context.Guild.GetUser(Context.User.Id));
            
            await Context.Channel.SendFileAsync(image, $"Kaguya_Profile_" + $"{Context.User.Username}_{DateTime.Now.Month}_" +
                                                       $"{DateTime.Now.Day}_{DateTime.Now.Year}.png");
        }
    }
}
