using System;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class AddFish : KaguyaBase
    {
        [OwnerCommand]
        [Command("AddFish")]
        [Summary("Gives the user a fish of the specified type")]
        [Remarks("<fish string> <user> <points> [count] [exp]")]
        public async Task Command(string fishStr, SocketGuildUser target, int pointsVal, int count = 1, int exp = 0)
        {
            User user = await DatabaseQueries.GetOrCreateUserAsync(target.Id);
            var fishEnum = (FishType) Enum.Parse(typeof(FishType), fishStr.ToUpper());

            var r = new Random();
            for (int i = 0; i < count; i++)
            {
                int fId = r.Next(0, 9999999);
                var fish = new Fish
                {
                    FishId = fId,
                    UserId = target.Id,
                    ServerId = Context.Guild.Id,
                    TimeCaught = DateTime.MinValue.ToOADate(),
                    FishType = fishEnum,
                    FishString = fishEnum.Humanize(),
                    Value = pointsVal,
                    Exp = exp,
                    Sold = false
                };

                await DatabaseQueries.InsertAsync(fish);
            }

            var sb = new StringBuilder();
            sb.AppendLine($"{Context.User.Mention} `{count:N0}` Fish added into database:");
            sb.AppendLine($"UserId: `{target.Id}`");
            sb.AppendLine($"ServerId: `{Context.Guild.Id}`\n");
            sb.AppendLine($"FishType: `{fishEnum}`");
            sb.AppendLine($"Exp: `{exp}`");
            sb.AppendLine($"Value: `{pointsVal}`");

            var embed = new KaguyaEmbedBuilder
            {
                Description = sb.ToString()
            };

            await SendEmbedAsync(embed);
        }
    }
}