using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Humanizer;
using Humanizer.Localisation;
using KaguyaProjectV2.KaguyaBot.Core.Application;
using KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Services.OwnerGiveawayServices
{
    /// <summary>
    /// The purpose of this class is to periodically update currently active owner-sponsored point/exp
    /// giveaways (again, reaction-based giveaways).
    /// </summary>
    public class OwnerGiveawayMessageUpdaterService
    {
        private static IEnumerable<OwnerGiveaway> _ownerGiveawayCache = MemoryCache.OwnerGiveawaysCache;
        private static IEnumerable<OwnerGiveaway> _activeGiveaways =
            _ownerGiveawayCache.Where(x => !x.HasExpired);
        
        public static async Task Initialize(int milliseconds = 15000) // 15 seconds
        {
            var timer = new Timer(milliseconds);
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += async (s, e) =>
            {
                _ownerGiveawayCache = MemoryCache.OwnerGiveawaysCache;
                _activeGiveaways = _ownerGiveawayCache.Where(x => !x.HasExpired).ToHashSet();

                if (!_activeGiveaways.Any())
                    return;
                
                foreach (var giveaway in _activeGiveaways)
                {
                    var guildChannel = KaguyaBase.Client.GetChannel(giveaway.ChannelId);
                    if (guildChannel == null)
                    {
                        await ConsoleLogger.LogAsync("Failed to find channel for active reaction role " +
                                                     "giveaway!!", LogLvl.ERROR);
                        return;
                    }

                    if (!(guildChannel is SocketTextChannel textCh))
                    {
                        await ConsoleLogger.LogAsync($"The owner giveaway with message ID {giveaway.MessageId} " +
                                                     $"and channel ID {giveaway.ChannelId} did not have a " +
                                                     $"guildChannel marked as SocketTextChannel. Removing from database!", LogLvl.WARN);
                        await DatabaseQueries.DeleteAsync(giveaway);
                        continue;
                    }
                    IMessage msg = await textCh.GetMessageAsync(giveaway.MessageId);
                    IUserMessage userMessage = msg as IUserMessage;
                    
                    if (userMessage == null)
                    {
                        await ConsoleLogger.LogAsync(
                            $"Failed to update owner giveaway message with message ID {giveaway.MessageId}. The " +
                            "RestUserMessage could not be found.", LogLvl.WARN);
                        
                        await DatabaseQueries.DeleteAsync(giveaway);
                        await ConsoleLogger.LogAsync($"Owner giveaway with message ID {giveaway.MessageId} " +
                                                     $"removed from database.", LogLvl.WARN);
                        continue;
                    }

                    if (String.IsNullOrWhiteSpace(userMessage.Content) && !userMessage.Embeds.Any())
                    {
                        await ConsoleLogger.LogAsync($"Ownergiveaway with message ID {giveaway.MessageId} did not " +
                                                     "have any content or any embeds. Removing from Database.", LogLvl.WARN);
                        await DatabaseQueries.DeleteAsync(giveaway);
                        continue;
                    }
                    
                    await userMessage.ModifyAsync(async m =>
                    {
                        try
                        {
                            var cacheEmbed = userMessage.Embeds.FirstOrDefault();

                            if (cacheEmbed == null)
                            {
                                await ConsoleLogger.LogAsync("The embed for an owner giveaway message was null!!",
                                    LogLvl.WARN);
                                return;
                            }
                            var embed = new KaguyaEmbedBuilder(EmbedColor.GOLD)
                            {
                                Title = cacheEmbed.Title,
                                Description = cacheEmbed.Description
                            };
                        
                            bool pointsGiveaway = giveaway.Points > 0;
                            bool expGiveaway = giveaway.Exp > 0;
                        
                            var descSb = OwnerGiveawayCommand.DescriptionStringBuilder("1s", pointsGiveaway,
                                expGiveaway,
                                giveaway.Points, giveaway.Exp, out TimeSpan timeSpan);

                            string lastLine = descSb.ToString().Split("\r\n")[^2];
                            if (!giveaway.HasExpired && giveaway.Expiration < DateTime.Now.ToOADate() || 
                                giveaway.Expiration < DateTime.Now.ToOADate())
                            {
                                descSb = descSb.Replace(lastLine, "This giveaway has ended.");

                                giveaway.HasExpired = true;
                                await DatabaseQueries.UpdateAsync(giveaway);
                            }
                            else
                            {
                                string humanizedTimeRemaining = (DateTime.FromOADate(giveaway.Expiration) - DateTime.Now).Humanize(2, minUnit: TimeUnit.Second);
                                string endString = $"This giveaway will end in `{humanizedTimeRemaining}`.";
                                
                                descSb = descSb.Replace(lastLine, endString);
                            }

                            embed.Description = descSb.ToString();
                            m.Embed = embed.Build();
                        }
                        catch (TimeoutException tException)
                        {
                            await ConsoleLogger.LogAsync(tException,
                                "An unexpected timeout exception was thrown in the " +
                                "owner giveaway message updater service.");
                        }
                    });
                }
            };
        }
    }
}