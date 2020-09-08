using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Application;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class OwnerGiveawayCommand : KaguyaBase
    {
        [OwnerCommand]
        [Command("OwnerGiveaway")]
        [Summary("Allows a bot owner to start a giveaway for `points` and `exp`. If you don't want " +
                 "to giveaway either points OR exp, set that value to 0.\n\n" +
                 "A reaction will be added to the message. Anyone who clicks the reaction will be given " +
                 "the specified amount of points or exp. Limit 1 award per reaction.")]
        [Remarks("<duration> <points> <exp>\n2h 0 500\n5h 750 250")]
        public async Task Command(string duration, int points, int exp)
        {
            if (!Emote.TryParse("<:Kaguya:581581938884608001>", out Emote reactionEmote))
            {
                throw new Exception("Emote could not be parsed.");
            }

            bool pointsGiveaway = points > 0;
            bool expGiveaway = exp > 0;
            
            StringBuilder titleSb = TitleStringBuilder(pointsGiveaway, expGiveaway);
            StringBuilder descriptionSb = DescriptionStringBuilder(duration, pointsGiveaway, expGiveaway, points, exp, 
                out TimeSpan giveawayTimespan);
            
            await Context.Message.DeleteAsync(); // Delete message from bot owner.
            
            var embed = new KaguyaEmbedBuilder(EmbedColor.GOLD)
            {
                Title = titleSb.ToString(),
                Description = descriptionSb.ToString()
            };

            var embedMsg = await SendEmbedAsync(embed);
            await embedMsg.AddReactionAsync(reactionEmote);
            
            var giveawayObj = new OwnerGiveaway
            {
                MessageId = embedMsg.Id,
                ChannelId = embedMsg.Channel.Id,
                Exp = exp,
                Points = points,
                Expiration = DateTime.Now.AddSeconds(giveawayTimespan.TotalSeconds).ToOADate(),
                HasExpired = false
            };
            int id = await DatabaseQueries.InsertWithIdentityAsync(giveawayObj);
            var cachedObj = giveawayObj;
            cachedObj.Id = id;
            MemoryCache.OwnerGiveawaysCache.Add(cachedObj);
            
            await ConsoleLogger.LogAsync($"Owner giveaway created.", LogLvl.DEBUG);
        }

        private static StringBuilder TitleStringBuilder(bool pointsGiveaway, bool expGiveaway)
        {
            var titleSb = new StringBuilder("Kaguya ");
            if (pointsGiveaway)
                titleSb.Append("Points ");
            if (pointsGiveaway && expGiveaway)
                titleSb.Append("+ ");
            if (expGiveaway)
                titleSb.Append("Exp ");
            titleSb.Append("Giveaway!");
            return titleSb;
        }

        public static StringBuilder DescriptionStringBuilder(string duration, bool pointsGiveaway, bool expGiveaway, 
            int points, int exp, out TimeSpan parsedTimeSpan)
        {
            var timeSpan = duration.ParseToTimespan();
            
            var descSb = new StringBuilder("Click the reaction below to gain:\r\n");
            if (pointsGiveaway)
                descSb.AppendLine($"- `{points:N0} points`");
            if (expGiveaway)
                descSb.AppendLine($"- `{exp:N0} exp`");

            descSb.AppendLine();
            descSb.AppendLine($"This giveaway will end in `{timeSpan.Humanize(2)}`.");

            parsedTimeSpan = timeSpan;
            return descSb;
        }
    }
}