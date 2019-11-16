using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Log;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class SetLogChannel : ModuleBase<ShardedCommandContext>
    {
        [Command("setlogchannel")]
        [Alias("log")]
        [Summary("Enables a list of given logtypes, and sends the log messages to a specific channel.")]
        [Remarks("<logtype> <channel>\ndeletedmessages #my-log-channel\nkaguyaserverlog.bans.unbans #my-admin-log-channel\ntwitchnotifications #live-streams")]
        public async Task SetChannel(string logType, IGuildChannel channel)
        {
            string[] logTypes = ArrayInterpreter.ReturnParams(logType);
            Server server = ServerQueries.GetServer(Context.Guild.Id);
            
            foreach(var type in logTypes)
            {
                await Logger.Log($"Server has set log type: [ID: {Context.Guild.Id} | Type: {type.ToUpperInvariant()}]", DataStorage.JsonStorage.LogLevel.DEBUG);
                switch (type.ToLower())
                {
                    case "kaguyaserverlog": server.LogKaguyaServerLog = channel.Id; break;
                    case "deletedmessages": server.LogDeletedMessages = channel.Id; break;
                    case "updatedmessages": server.LogUpdatedMessages = channel.Id; break;
                    case "userjoins": server.LogUserJoins = channel.Id; break;
                    case "userexits": server.LogUserLeaves = channel.Id; break;
                    case "antiraid": server.LogAntiraids = channel.Id; break;
                    case "kicks": server.LogKicks = channel.Id; break;
                    case "bans": server.LogBans = channel.Id; break;
                    case "unbans": server.LogUnbans = channel.Id; break;
                    case "filteredphrases": server.LogFilteredPhrases = channel.Id; break;
                    case "userconnectstovoice": server.LogVoiceChannelConnections = channel.Id; break;
                    case "userdisconnectsfromvoice": server.LogVoiceChannelDisconnections = channel.Id; break;
                    case "levelups": server.LogLevelAnnouncements = channel.Id; break;
                    case "shadowbans": server.LogShadowbans = channel.Id; break;
                    case "unshadowbans": server.LogUnshadowbans = channel.Id; break;
                    case "warns": server.LogWarns = channel.Id; break;
                    case "unwarns": server.LogUnwarns = channel.Id; break;
                    case "twitchnotifications": server.LogTwitchNotifications = channel.Id; break;
                    case "youtubenotifications": server.LogYouTubeNotifications = channel.Id; break;
                    case "redditnotifications": server.LogRedditNotifications = channel.Id; break;
                    case "twitternotifications": server.LogTwitterNotifications = channel.Id; break;
                    case "all":
                        {
                            server.LogKaguyaSettingChanges = channel.Id;
                            server.LogDeletedMessages = channel.Id;
                            server.LogUpdatedMessages = channel.Id;
                            server.LogUserJoins = channel.Id;
                            server.LogUserLeaves = channel.Id;
                            server.LogAntiraids = channel.Id;
                            server.LogKicks = channel.Id;
                            server.LogBans = channel.Id;
                            server.LogUnbans = channel.Id;
                            server.LogFilteredPhrases = channel.Id;
                            server.LogVoiceChannelConnections = channel.Id;
                            server.LogVoiceChannelDisconnections = channel.Id;
                            server.LogLevelAnnouncements = channel.Id;
                            server.LogShadowbans = channel.Id;
                            server.LogUnshadowbans = channel.Id;
                            server.LogWarns = channel.Id;
                            server.LogUnwarns = channel.Id;
                            server.LogTwitchNotifications = channel.Id;
                            server.LogYouTubeNotifications = channel.Id;
                            server.LogRedditNotifications = channel.Id;
                            server.LogTwitterNotifications = channel.Id;
                        }
                        break;
                }
            }

            ServerQueries.UpdateServer(server);

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully set logtype `{string.Join(", ", logTypes)}` to channel `#{channel.Name}`"
            };

            await ReplyAsync(embed: embed.Build());
        }
    }
}
