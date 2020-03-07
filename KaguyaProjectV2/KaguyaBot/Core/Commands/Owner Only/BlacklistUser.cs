using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class BlacklistUser : KaguyaBase
    {
        [Command("BlacklistUser")]
        [Alias("UserBlacklist", "ubl")]
        [Summary("Allows a bot owner to blacklist a user temporarily.")]
        [Remarks("<id> <duration> [reason]")]

        public async Task Command(ulong id, string duration, [Remainder]string reason = "No reason provided.")
        {
            var target = await DatabaseQueries.GetOrCreateUserAsync(id);
            var socketTarget = Client.GetUser(target.UserId);

            var expiration = (DateTime.Now + duration.ParseToTimespan()).ToOADate();
            var humanizedExpiration = duration.ParseToTimespan().Humanize(3);

            if (target.IsBlacklisted)
            {
                await SendBasicErrorEmbedAsync($"This user is already blacklisted.\n\n" +
                                               $"Reason: `{target.Blacklist.Reason}`\n" +
                                               $"Expires `{DateTime.FromOADate(target.Blacklist.Expiration).Humanize()}`");
                return;
            }

            var blacklist = new UserBlacklist
            {
                UserId = target.UserId,
                Expiration = expiration,
                Reason = reason,
                User = target
            };

            await DatabaseQueries.InsertAsync(blacklist);
            await ConsoleLogger.LogAsync($"User {target.UserId} has been blacklisted for " +
                                         $"{humanizedExpiration}", LogLvl.INFO);

            if(socketTarget != null && 
               await socketTarget.GetOrCreateDMChannelAsync() != null)
            {
                var embed = new KaguyaEmbedBuilder
                {
                    Title = "Kaguya Bot Blacklist",
                    Description = $"You have been blacklisted from the Kaguya Bot by an " +
                                  $"Administrator. During this time, you will not be able " +
                                  $"to use any Kaguya Commands, earn points, or earn EXP in any way.\n\n" +
                                  $"[[Appeal]](https://forms.gle/g9LmUZDD8KGchDXYA)",
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Reason",
                            Value = reason
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Duration",
                            Value = humanizedExpiration
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Administrator",
                            Value = Context.User.ToString()
                        }
                    }
                };

                await (await socketTarget.GetOrCreateDMChannelAsync()).SendEmbedAsync(embed);
            }

            await SendBasicSuccessEmbedAsync($"Successfully blacklisted user " +
                                             $"`{socketTarget?.ToString() ?? target.UserId.ToString()}` " +
                                             $"for `{humanizedExpiration}`.");
        }
    }
}
