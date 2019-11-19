using Discord;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration.LogCommands
{
    public class LogQuery
    {
        /// <summary>
        /// Performs all necessary actions that allow a server to have their logtypes enabled or disabled.
        /// </summary>
        /// <param name="args">A period separated list of logtypes to enable/disable.</param>
        /// <param name="channel">The channel where we will be sending the log messages to.</param>
        /// <param name="enabled">A boolean stating whether we should enable the logging or disable it.</param>
        public static async Task<List<string>> LogSwitcher(string args, bool enabled, ulong guildId, IGuildChannel channel = null)
        {
            List<string> logTypes = ArrayInterpreter.ReturnParams(args).ToList();
            Server server = ServerQueries.GetServer(guildId);

            foreach (var type in logTypes.ToList())
            {
                if (enabled)
                {
                    await ConsoleLogger.Log($"Server has set log type: [ID: {channel.GuildId} | Type: {type.ToUpperInvariant()}]", LogLevel.DEBUG);
                    switch (type.ToLower())
                    {
                        case "modlog":
                        {
                            if (server.IsPremium)
                            {
                                server.ModLog = channel.Id;
                            }
                            break;
                        }
                        case "deletedmessages": server.LogDeletedMessages = channel.Id; break;
                        case "updatedmessages": server.LogUpdatedMessages = channel.Id; break;
                        case "userjoins": server.LogUserJoins = channel.Id; break;
                        case "userleaves": server.LogUserLeaves = channel.Id; break;
                        case "antiraid": server.LogAntiraids = channel.Id; break;
                        case "kicks": server.LogKicks = channel.Id; break;
                        case "bans": server.LogBans = channel.Id; break;
                        case "unbans": server.LogUnbans = channel.Id; break;
                        case "filteredphrases": server.LogFilteredPhrases = channel.Id; break;
                        case "uservoiceconnectionupdated": server.LogVoiceChannelConnections = channel.Id; break;
                        case "levelups": server.LogLevelAnnouncements = channel.Id; break;
                        case "twitchnotifications": server.LogTwitchNotifications = channel.Id; break;
                        case "youtubenotifications": server.LogYouTubeNotifications = channel.Id; break;
                        case "redditnotifications": server.LogRedditNotifications = channel.Id; break;
                        case "twitternotifications": server.LogTwitterNotifications = channel.Id; break;
                        case "all":
                            {
                                server.ModLog = channel.Id;
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
                                server.LogLevelAnnouncements = channel.Id;
                                server.LogTwitchNotifications = channel.Id;
                                server.LogYouTubeNotifications = channel.Id;
                                server.LogRedditNotifications = channel.Id;
                                server.LogTwitterNotifications = channel.Id;
                            }
                            break;
                        default: logTypes.Remove(type); break;
                    }
                }
                else
                {
                    await ConsoleLogger.Log($"Server has disabled log type: [ID: {guildId} | Type: {type.ToUpperInvariant()}]", LogLevel.DEBUG);
                    switch (type.ToLower())
                    {
                        case "kaguyaserverlog": server.ModLog = 0; break;
                        case "deletedmessages": server.LogDeletedMessages = 0; break;
                        case "updatedmessages": server.LogUpdatedMessages = 0; break;
                        case "userjoins": server.LogUserJoins = 0; break;
                        case "userleaves": server.LogUserLeaves = 0; break;
                        case "antiraid": server.LogAntiraids = 0; break;
                        case "kicks": server.LogKicks = 0; break;
                        case "bans": server.LogBans = 0; break;
                        case "unbans": server.LogUnbans = 0; break;
                        case "filteredphrases": server.LogFilteredPhrases = 0; break;
                        case "userconnectstovoice": server.LogVoiceChannelConnections = 0; break;
                        case "levelups": server.LogLevelAnnouncements = 0; break;
                        case "twitchnotifications": server.LogTwitchNotifications = 0; break;
                        case "youtubenotifications": server.LogYouTubeNotifications = 0; break;
                        case "redditnotifications": server.LogRedditNotifications = 0; break;
                        case "twitternotifications": server.LogTwitterNotifications = 0; break;
                        case "all":
                            {
                                server.ModLog = 0;
                                server.LogDeletedMessages = 0;
                                server.LogUpdatedMessages = 0;
                                server.LogUserJoins = 0;
                                server.LogUserLeaves = 0;
                                server.LogAntiraids = 0;
                                server.LogKicks = 0;
                                server.LogBans = 0;
                                server.LogUnbans = 0;
                                server.LogFilteredPhrases = 0;
                                server.LogVoiceChannelConnections = 0;
                                server.LogLevelAnnouncements = 0;
                                server.LogTwitchNotifications = 0;
                                server.LogYouTubeNotifications = 0;
                                server.LogRedditNotifications = 0;
                                server.LogTwitterNotifications = 0;
                            }
                            break;
                        default: logTypes.Remove(type); break;
                    }
                }
            }
            ServerQueries.UpdateServer(server);
            return logTypes;
        }
    }
}
