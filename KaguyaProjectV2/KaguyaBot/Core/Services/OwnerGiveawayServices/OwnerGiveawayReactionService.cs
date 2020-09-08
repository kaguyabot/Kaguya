using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Application;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Services.OwnerGiveawayServices
{
    public class OwnerGiveawayReactionService
    {
        // Un-expired owner giveaways.
        private static IEnumerable<OwnerGiveaway> _ownerGiveawayCache = MemoryCache.OwnerGiveawaysCache;
        private static IEnumerable<OwnerGiveaway> _validCache;
        private static HashSet<ulong> _validIds;

        private static IEnumerable<OwnerGiveawayReaction> _previousReactions;
        public static async Task ReactionAdded(Cacheable<IUserMessage, ulong> cache,
            ISocketMessageChannel channel, SocketReaction reaction)
        {
            _ownerGiveawayCache = MemoryCache.OwnerGiveawaysCache;
            _previousReactions = MemoryCache.OwnerGiveawayReactions;

            _validCache = _ownerGiveawayCache.Where(x => !x.HasExpired);
            _validIds = _validCache.Select(x => x.MessageId).ToHashSet();
            
            ulong msgId = _validIds.FirstOrDefault(x => x == reaction.MessageId);
            bool giveawayReaction = msgId != 0;

            if (!giveawayReaction)
                return;
            
            var validGiveaway = _ownerGiveawayCache.First(x => x.MessageId == msgId);

            if (_previousReactions == null)
                return;
            
            if (!reaction.User.IsSpecified || reaction.User.Value.IsBot || validGiveaway == null)
                return;
            
            if(_previousReactions.Any(x => x.UserId == reaction.UserId && x.OwnerGiveawayId == validGiveaway.Id))
                return;
            
            var user = await DatabaseQueries.GetOrCreateUserAsync(reaction.UserId);

            await ConsoleLogger.LogAsync("Owner-only giveaway awarding triggered for " +
                                         $"user {user.UserId}", LogLvl.DEBUG);
            
            user.Points += validGiveaway.Points;
            user.Experience += validGiveaway.Exp;

            var ownerGiveawayReaction = new OwnerGiveawayReaction
            {
                OwnerGiveawayId = validGiveaway.Id,
                UserId = user.UserId
            };

            await ConsoleLogger.LogAsync("Owner-only giveaway reaction registered " +
                                         $"for user {user.UserId}.", LogLvl.DEBUG);
            
            MemoryCache.OwnerGiveawayReactions.Add(ownerGiveawayReaction);
            
            await DatabaseQueries.InsertAsync(ownerGiveawayReaction);
            await DatabaseQueries.UpdateAsync(user);
        }
    }
}