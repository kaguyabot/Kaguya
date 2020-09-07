using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Application;
using KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
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
            _ownerGiveawayCache.Where(x => x.Expiration > DateTime.Now.ToOADate());
        
        public static async Task Initialize(int milliseconds = 5000) // 30 seconds
        {
            var timer = new Timer(milliseconds);
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += async (s, e) =>
            {
                _ownerGiveawayCache = MemoryCache.OwnerGiveawaysCache;
                _activeGiveaways = _ownerGiveawayCache.Where(x => x.Expiration > DateTime.Now.ToOADate());
                
                foreach (var giveaway in _activeGiveaways)
                {
                    var guildChannel = KaguyaBase.Client.GetChannel(giveaway.ChannelId);
                    if (guildChannel == null)
                    {
                        await ConsoleLogger.LogAsync("Failed to find channel for active reaction role " +
                                                     "giveaway!!", LogLvl.ERROR);
                        return;
                    }
                    
                    var msg = await (guildChannel as SocketTextChannel).GetMessageAsync(giveaway.MessageId);
                    var userMessage = msg as RestUserMessage;
                    
                    if (userMessage == null)
                    {
                        await ConsoleLogger.LogAsync(
                            $"Failed to update owner giveaway message with ID {giveaway.MessageId}. The " +
                            $"RestUserMessage could not be found.", LogLvl.WARN);
                        continue;
                    }
                    
                    await userMessage.ModifyAsync(msg =>
                    {
                        if (!msg.Content.IsSpecified && !msg.Embed.IsSpecified)
                            return;
                        
                        var cacheEmbed = msg.Embed.Value;
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
                    
                        embed.Description = descSb.ToString().Replace("1s", 
                            (DateTime.FromOADate(giveaway.Expiration) - DateTime.Now).Humanize(2));
                    
                        msg.Embed = embed.Build();
                    });
                }
            };
        }
    }
}