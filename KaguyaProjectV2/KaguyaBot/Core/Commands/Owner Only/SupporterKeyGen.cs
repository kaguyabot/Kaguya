using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class SupporterKeyGen : ModuleBase<ShardedCommandContext>
    {
        private static readonly Random Random = new Random();

        [RequireOwner]
        [Command("gen", RunMode = RunMode.Async)]
        [Summary("Generates a specified amount of Kaguya Supporter " +
                 "keys for the length of time given (in days). If no amount is " +
                 "specified, this command generates 1 key. If no length of time is given, " +
                 "we will generate a 30 day key.")]
        [Remarks(" => One 30-day key\n<amount> <time in days>\n90 5 => Generates five 90-day keys")]
        public async Task GenerateKeys(int amount = 0, int timeInDays = 0)
        {
            if (timeInDays == 0) timeInDays = 30;
            if (amount == 0) amount = 1;

            var existingKeys = UtilityQueries.GetAllKeys();
            List<SupporterKey> keys = new List<SupporterKey>();

            for (int i = 0; i < amount; i++)
            {
                var key = new SupporterKey
                {
                    Key = RandomString(),
                    Length = timeInDays
                };

                if (existingKeys.Exists(x => x.Key == key.Key))
                {
                    continue;
                }

                keys.Add(key);
            }

            UtilityQueries.AddKeys(keys);
            await ChatReply(timeInDays, amount);
        }

        private async Task ChatReply(int timeInDays, int amount)
        {
            string s = "";
            if (amount != 1)
                s = "s";

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully generated `{amount}` `{timeInDays} day` key{s}." +
                              $"\nThese new keys have been added to the database."
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