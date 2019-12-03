using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Global;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class Redeem : ModuleBase<ShardedCommandContext>
    {
        [HelpCommand]
        [Command("redeem")]
        [Summary("Allows a user to redeem a Kaguya Supporter key. Supporter Keys may be " +
                 "purchased [at this link](https://stageosu.selly.store/)")]
        [Remarks("<key>")]
        public async Task RedeemKey(string userKey)
        {
            var user = UserQueries.GetUser(Context.User.Id);
            var server = ServerQueries.GetServer(Context.Guild.Id);
            var existingKeys = UtilityQueries.GetAllKeys();

            var key = existingKeys.FirstOrDefault(x => x.Key == userKey && x.UserId == 0);

            if (key == null)
            {
                var embed0 = new KaguyaEmbedBuilder
                {
                    Description = "Key does not exist or has already been redeemed.",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"If you need assistance, please join the server provided in {server.CommandPrefix}invite"
                    }

                };
                embed0.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: embed0.Build());
                return;
            }

            var newKey = new SupporterKey
            {
                Key = key.Key,
                LengthInSeconds = key.LengthInSeconds,
                LengthInDays = key.LengthInDays,
                KeyCreatorId = key.KeyCreatorId,
                UserId = Context.User.Id,
                Expiration = DateTime.Now.AddSeconds(key.LengthInSeconds).ToOADate()
            };

            UtilityQueries.UpdateKey(key, newKey);

            TimeSpan ts = RegexTimeParser.ParseToTimespan($"{newKey.LengthInSeconds}s");

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully redeemed `" +
                              $"{RegexTimeParser.FormattedTimeString(ts.Seconds, ts.Minutes, ts.Hours, ts.Days)}` " +
                              $"of Kaguya Supporter!\n" +
                              $"Your tag will expire on: `{DateTime.FromOADate(user.SupporterExpirationDate).ToLongDateString()}`"
            };
            embed.SetColor(EmbedColor.GOLD);

            await ReplyAsync(embed: embed.Build());
            await SendEmbedToBotOwner(Context, newKey);
        }

        private async Task SendEmbedToBotOwner(ICommandContext context, SupporterKey key)
        {
            var owner = ConfigProperties.client.GetUser(ConfigProperties.botOwnerId);
            var fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder {IsInline = false, Name = "Key Properties", 
                    Value = $"Key: `{key.Key}`\nCreated by: `{owner}`\nExpires `{DateTime.FromOADate(key.Expiration).Humanize(false)}`"}
            };

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"User `[Name: {context.User}` has just redeemed a " +
                              $"Kaguya Supporter key!",
                Fields = fields
            };

            await owner.SendMessageAsync(embed: embed.Build());
        }
    }
}
