using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class SupporterKeyGen : ModuleBase<ShardedCommandContext>
    {
        private static readonly Random Random = new Random();

        [OwnerCommand]
        [Command("supportergen")]
        [Summary("Generates a specified amount of Kaguya Supporter " +
                 "keys for the length of time given (in days). If no amount is " +
                 "specified, this command generates 1 key. If no length of time is given, " +
                 "we will generate a 30 day key.")]
        [Remarks(" => One 30-day key\n<amount> <time in days>")]
        public async Task GenerateKeys(int amount, string duration)
        {
            if(amount < 1)
                throw new IndexOutOfRangeException("Amount parameter must be greater than one.");

            RegexTimeParser.Parse(duration, out int sec, out int min, out int hour, out int day);

            TimeSpan timeSpan = RegexTimeParser.ParseToTimespan(duration);
            long timeInSeconds = (long)timeSpan.TotalSeconds;

            var existingKeys = await DatabaseQueries.GetAllAsync<SupporterKey>(x =>
                x.Expiration > DateTime.Now.ToOADate() &&
                x.UserId != 0);
            List<SupporterKey> keys = new List<SupporterKey>();

            for (int i = 0; i < amount; i++)
            {
                var key = new SupporterKey
                {
                    Key = RandomString(),
                    LengthInSeconds = timeInSeconds,
                    KeyCreatorId = Context.User.Id
                };

                if (existingKeys.Exists(x => x.Key == key.Key))
                {
                    continue;
                }

                keys.Add(key);
            }

            using (var memoryStream = new MemoryStream())
            {
                var writer = new StreamWriter(memoryStream);

                foreach (var key in keys)
                {
                    writer.Write($"{key.Key}\n");
                }

                await writer.FlushAsync();
                memoryStream.Seek(0, SeekOrigin.Begin);

                await Context.User.SendFileAsync(memoryStream, $"{keys.Count} Keys.txt");
            }

            await DatabaseQueries.BulkCopy(keys);
            await ChatReply(RegexTimeParser.FormattedTimeString(duration), amount);
        }

        private async Task ChatReply(string formattedTimeString, int amount)
        {
            #region Grammar

            string s = "";

            if (amount != 1)
            {
                s = "s";
            }

            #endregion

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully generated `{amount} key{s}`.\n\n" +
                              $"Duration: `{formattedTimeString}`\n" +
                              $"Creator: `[Name: {Context.User} | ID: {Context.User.Id}]`\n"
            };
            await ReplyAsync(embed: embed.Build());
        }

        // We could use a prettier LINQ expression, but this is twice as fast.
        public static string RandomString(int length = 20)
        {
            const string chars = @"AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789!@#$%^&()+=-\{}[]';~";
            var stringChars = new char[length];

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[Random.Next(chars.Length)];
            }

            return new string(stringChars);
        }
    }
}