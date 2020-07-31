using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    public class ReactionHandler
    {
        private readonly bool CacheTimerEnabled = false;
        private List<ReactionRole> _reactionRoleCache;

        public ReactionHandler()
        {
            ConsoleLogger.LogAsync("Populating reaction role cache...", LogLvl.DEBUG);
            // We have this as synchronus during initial setup.
            _reactionRoleCache ??= GetReactionRoleCache().Result;
            ConsoleLogger.LogAsync("Reaction role cache populated.", LogLvl.DEBUG);

            if (!CacheTimerEnabled)
            {
                CacheTimerEnabled = true;

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
        public async Task ReactionAdded(Cacheable<IUserMessage, ulong> cache,
            ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!(channel is SocketGuildChannel guildChannel))
                return; // The reaction was sent via DM, return.
            if (!reaction.User.IsSpecified)
                return;

            IMessage msg;

            Emote emote = (Emote)reaction.Emote;
            if (emote == null)
                return;
            
            if (!reaction.Message.IsSpecified)
            {
                msg = await ((SocketTextChannel) guildChannel).GetMessageAsync(reaction.MessageId);
            }
            else
            {
                msg = reaction.Message.Value;
            }
            
            var rrCacheMatch = _reactionRoleCache.FirstOrDefault(x => x.MessageId == msg.Id &&
                                                                      x.EmoteId == emote.Id);
            if (rrCacheMatch == null)
                return;

            var guild = guildChannel.Guild;
            if(!guild.Roles.Any(x => x.Id == rrCacheMatch.RoleId))
            {
                var rrLogSb = new StringBuilder();
                rrLogSb.Append($"[Server ID: {rrCacheMatch.ServerId} | ");
                rrLogSb.Append($"Role ID: {rrCacheMatch.RoleId} | ");
                rrLogSb.Append($"Emote ID: {rrCacheMatch.EmoteId} | ");
                rrLogSb.Append($"Message ID: {rrCacheMatch.MessageId}]");

                _reactionRoleCache.Remove(rrCacheMatch);
                await DatabaseQueries.DeleteAsync(rrCacheMatch);
                await ConsoleLogger.LogAsync($"Deleted reaction role with properties {rrLogSb} from database because " +
                                       $"the role no longer exists.", LogLvl.DEBUG);
                return;
            }

            var role = guild.Roles.First(x => x.Id == rrCacheMatch.RoleId);
            var user = reaction.User.Value as SocketGuildUser;

            if (user == null) 
                return; // We sort of already check for this above, this is here just to be safe.
            
            if(user.Roles.Any(x => x.Id == role.Id))
                return; // The user already has the role, no need to re-assign.
            
            

            try
            {
                await user.AddRoleAsync(role);
                await ConsoleLogger.LogAsync($"User {user.Id} has been given role [Name: {role.Name} | " +
                                             $"ID: {role.Id}] in guild [Name: {guild.Name} | {guild.Id}] via " +
                                             $"a Reaction Role.", LogLvl.DEBUG);
            }
            catch (Exception e)
            {
                await ConsoleLogger.LogAsync(e, LogLvl.ERROR);
            }
        }

        private static async Task<List<ReactionRole>> GetReactionRoleCache()
            => await DatabaseQueries.GetAllAsync<ReactionRole>();
    }
}