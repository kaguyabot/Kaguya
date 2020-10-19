using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class AddPoints : KaguyaBase
    {
        [OwnerCommand]
        [Command("AddPoints")]
        [Summary("Allows the bot owner to add points to a user. " +
                 "The amount of points specified must be a positive number.")]
        [Remarks("<points amount> <user ID>")]
        public async Task Command(int points, ulong userId)
        {
            User user = await DatabaseQueries.GetOrCreateUserAsync(userId);

            SocketUser discUser = Client.GetUser(userId);
            string nameString = discUser != null ? $"{discUser.Username}'s" : userId.ToString();

            var embed = new KaguyaEmbedBuilder(EmbedColor.GREEN)
            {
                Title = "Add Points",
                Description = $"Successfully updated user {nameString} points.\n\n" +
                              $"Old points value: `{user.Points:N0}`\n" +
                              $"New points value: `{user.Points + points:N0}`"
            };

            user.AddPoints(points);

            await DatabaseQueries.UpdateAsync(user);
            await SendEmbedAsync(embed);
        }
    }
}