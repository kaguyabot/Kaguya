using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Osu;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using OsuSharp;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.osu
{
    public class OsuSet : KaguyaBase
    {
        [OsuCommand]
        [Command("osuSet")]
        [Summary("Allows a user to store their osu! username or ID in their Kaguya account. " +
                 "This way, users may use the other osu! commands without having to specify a " +
                 "player, assuming they want information about themselves.")]
        [Remarks("<username or ID>\nSomeUser\nSome user with spaces")]
        public async Task OsuSetCommand([Remainder] string username)
        {
            KaguyaEmbedBuilder embed;
            User playerObject = username.AsUlong(false) == 0
                ? await OsuBase.Client.GetUserByUsernameAsync(username, GameMode.Standard)
                : await OsuBase.Client.GetUserByUserIdAsync((long) username.AsUlong(), GameMode.Standard);

            if (playerObject == null)
            {
                await SendBasicErrorEmbedAsync($"The username you provided doesn't match an existing osu! player.");

                return;
            }

            //Getting user profile database object and updating it.
            DataStorage.DbData.Models.User user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);

            user.OsuId = (int) playerObject.UserId;
            await DatabaseQueries.UpdateAsync(user);

            embed = new KaguyaEmbedBuilder
            {
                Title = "osu! Username",
                Description = $"Your osu! username has been set to `{playerObject.Username}`."
            };

            await ReplyAsync(embed: embed.Build());
        }
    }
}