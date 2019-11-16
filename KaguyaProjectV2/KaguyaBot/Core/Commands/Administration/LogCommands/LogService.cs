using Discord;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Log;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration.LogCommands
{
    public class LogService
    {
        /// <summary>
        /// Performs all necessary actions that allow a server to have their logtypes enabled or disabled.
        /// </summary>
        /// <param name="args">A period separated list of logtypes to enable/disable.</param>
        /// <param name="channel">The channel where we will be sending the log messages to.</param>
        /// <param name="enabled">A boolean stating whether we should enable the logging or disable it.</param>
        public static async Task<List<string>> LogSwitcher(string args, IGuildChannel channel, bool enabled)
        {
            List<string> logTypes = ArrayInterpreter.ReturnParams(args).ToList();
            Server server = ServerQueries.GetServer(channel.GuildId);

            foreach (var type in logTypes.ToList())
            {
                await Logger.Log($"Server has set log type: [ID: {channel.GuildId} | Type: {type.ToUpperInvariant()}]", LogLevel.DEBUG);
                switch (type.ToLower())
                {
                    case "kaguyaserverlog": server.LogKaguyaServerLog = channel.Id; break;
                    case "deletedmessages": server.LogDeletedMessages = channel.Id; break;
                    case "updatedmessages": server.LogUpdatedMessages = channel.Id; break;
                    case "userjoins": server.LogUserJoins = channel.Id; break;
                    case "userleaves": server.LogUserLeaves = channel.Id; break;
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
                    default: logTypes.Remove(type); break;
                }
            }

            ServerQueries.UpdateServer(server);
            return logTypes;
        }
    }
}
