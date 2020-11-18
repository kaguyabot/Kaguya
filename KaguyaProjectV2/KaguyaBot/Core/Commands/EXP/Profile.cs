using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using JetBrains.Profiler.Api;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP
{
    public class Profile : KaguyaBase
    {
        [ExpCommand]
        [Command("Profile", RunMode = RunMode.Async)]
        [Alias("p")]
        [Summary("Displays your Kaguya Profile, showing off how much EXP you have earned, as well " +
                 "as some other stats!")]
        [Remarks("")]
        [RequireContext(ContextType.Guild)]
        public async Task Command(ulong? id = null)
        {
            id ??= Context.User.Id;

            User user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            if (id != Context.User.Id && !user.IsBotOwner)
            {
                await SendBasicErrorEmbedAsync("Only bot owners can display other people's profiles. Please " +
                                               "use this command without a parameter.");

                return;
            }

            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);

            if (id != Context.User.Id)
                user = await DatabaseQueries.GetOrCreateUserAsync(id.Value);

            var p = new ProfileImage();
            Stream image = await p.GenerateProfileImageStream(user, server, Context.Guild.GetUser(id.Value));

            using (Context.Channel.EnterTypingState())
            {
                await Context.Channel.SendFileAsync(image, "Kaguya_Profile_" +
                                                           $"{Context.User.Username}_{DateTime.Now.Month}_" +
                                                           $"{DateTime.Now.Day}_{DateTime.Now.Year}.png");
            }
        }
    }
}