using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Commands.Utility;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using MoreLinq.Extensions;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    public class ReactionRoleHandler
    {
        //todo: Have all cached variables be migrated to the MemoryCache class.
        private readonly bool _cacheTimerEnabled = false;
        private List<ReactionRole> _reactionRoleCache;

        public ReactionRoleHandler()
        {
            ConsoleLogger.LogAsync("Populating reaction role cache...", LogLvl.DEBUG);
            // We have this as synchronus during initial setup.
            _reactionRoleCache ??= GetReactionRoleCache().Result;
            ConsoleLogger.LogAsync("Reaction role cache populated.", LogLvl.DEBUG);

            CreateReactionRole.UpdatedCache += AddToCache;

            if (!_cacheTimerEnabled)
            {
                _cacheTimerEnabled = true;

                var t = new Timer(60000);
                t.AutoReset = true;
                t.Enabled = true;
                t.Elapsed += async (s, e) =>
                {
                    _reactionRoleCache = await GetReactionRoleCache();
                    await ConsoleLogger.LogAsync("Populated reaction role cache.", LogLvl.TRACE);
                };
            }
        }

        /// <summary>
        /// Handles the processing of reaction roles when a new reaction is added to a message.
        /// </summary>
        public async Task ReactionChanged(Cacheable<IUserMessage, ulong> cache,
            ISocketMessageChannel channel,
            SocketReaction reaction,
            bool added)
        {
            if (!(channel is SocketGuildChannel guildChannel))
                return; // The reaction was sent via DM, return.

            if (!reaction.User.IsSpecified)
                return;

            IMessage msg;
            IEmote emote = reaction.Emote;

            if (emote == null)
                return;

            bool isEmoji = emote is Emoji;

            if (!reaction.Message.IsSpecified)
                msg = await ((SocketTextChannel) guildChannel).GetMessageAsync(reaction.MessageId);
            else
                msg = reaction.Message.Value;

            ReactionRole rrCacheMatch;

            if (!isEmoji)
            {
                rrCacheMatch = _reactionRoleCache.FirstOrDefault(x => x.MessageId == msg.Id &&
                                                                      x.EmoteNameorId == (emote as Emote).Id.ToString());
            }
            else
            {
                rrCacheMatch = _reactionRoleCache.FirstOrDefault(x => x.MessageId == msg.Id &&
                                                                      x.EmoteNameorId == emote.Name);
            }

            if (rrCacheMatch == null)
                return;

            SocketGuild guild = guildChannel.Guild;
            if (!guild.Roles.Any(x => x.Id == rrCacheMatch.RoleId))
            {
                var rrLogSb = new StringBuilder();
                rrLogSb.Append($"[Server ID: {rrCacheMatch.ServerId} | ");
                rrLogSb.Append($"Role ID: {rrCacheMatch.RoleId} | ");
                rrLogSb.Append($"Emote ID: {rrCacheMatch.EmoteNameorId} | ");
                rrLogSb.Append($"Message ID: {rrCacheMatch.MessageId}]");

                _reactionRoleCache.Remove(rrCacheMatch);
                await DatabaseQueries.DeleteAsync(rrCacheMatch);
                await ConsoleLogger.LogAsync($"Deleted reaction role with properties {rrLogSb} from database because " +
                                             $"the role no longer exists.", LogLvl.DEBUG);

                return;
            }

            SocketRole role = guild.Roles.First(x => x.Id == rrCacheMatch.RoleId);
            var user = reaction.User.Value as SocketGuildUser;

            if (user == null || user.IsBot)
                return; // We sort of already check for this above, this is here just to be safe.

            if (added && user.Roles.Any(x => x.Id == role.Id))
                return; // The user already has the role, no need to re-assign.

            try
            {
                if (added)
                {
                    await user.AddRoleAsync(role);
                    await ConsoleLogger.LogAsync($"User {user.Id} has been given role [Name: {role.Name} | " +
                                                 $"ID: {role.Id}] in guild [Name: {guild.Name} | {guild.Id}] via " +
                                                 $"a Reaction Role.", LogLvl.DEBUG);
                }
                else
                {
                    await user.RemoveRoleAsync(role);
                    await ConsoleLogger.LogAsync($"User {user.Id} has had role [Name: {role.Name} | " +
                                                 $"ID: {role.Id}] removed from them in guild " +
                                                 $"[Name: {guild.Name} | {guild.Id}] via " +
                                                 $"a Reaction Role.", LogLvl.DEBUG);
                }
            }
            catch (Exception e)
            {
                await ConsoleLogger.LogAsync(e);
            }
        }

        private async Task<List<ReactionRole>> GetReactionRoleCache() => await DatabaseQueries.GetAllAsync<ReactionRole>();
        private void AddToCache(IEnumerable<ReactionRole> reactionRoles) => _reactionRoleCache.AddRange(reactionRoles);
        private void AddToCache(ReactionRole reactionRole) => _reactionRoleCache.Add(reactionRole);
        private void RemoveFromCache(IEnumerable<ReactionRole> reactionRoles) => reactionRoles.ForEach(x => _reactionRoleCache.Remove(x));
        private void RemoveFromCache(ReactionRole reactionRole) => _reactionRoleCache.Remove(reactionRole);
    }
}