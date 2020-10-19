using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration.LogCommands
{
    public static class LogQuery
    {
        public static string[] AllLogTypes =
        {
            "ModLog",
            "DeletedMessages",
            "UpdatedMessages",
            "FilteredPhrases",
            "UserJoins",
            "UserLeaves",
            "Bans",
            "Unbans",
            "VoiceConnections",
            "LevelAnnouncements",
            "FishLevels",
            "AntiRaid",
            "Greetings",
            "All"
        };

        /// <summary>
        /// Performs all necessary actions that allow a server to have their logtypes enabled or disabled.
        /// </summary>
        /// <param name="args">A period separated list of logtypes to enable/disable.</param>
        /// <param name="channel">The channel where we will be sending the log messages to.</param>
        /// <param name="enabled">A boolean stating whether we should enable the logging or disable it.</param>
        public static async Task<List<string>> LogSwitcher(string args, bool enabled, ulong guildId, SocketTextChannel channel = null)
        {
            List<string> logTypes = ArrayInterpreter.ReturnParams(args).ToList();
            Server server = await DatabaseQueries.GetOrCreateServerAsync(guildId);

            foreach (string type in logTypes.ToList())
            {
                if (enabled)
                {
                    await ConsoleLogger.LogAsync($"Server has set log type: [ID: {channel.Guild.Id} | Type: {type.ToUpperInvariant()}]",
                        LogLvl.DEBUG);

                    switch (type.ToLower())
                    {
                        case "modlog":
                        {
                            if (server.IsPremium)
                                server.ModLog = channel.Id;
                            else
                                throw new KaguyaPremiumException();

                            break;
                        }
                        case "deletedmessages":
                            server.LogDeletedMessages = channel.Id;

                            break;
                        case "updatedmessages":
                            server.LogUpdatedMessages = channel.Id;

                            break;
                        case "filteredphrases":
                            server.LogFilteredPhrases = channel.Id;

                            break;
                        case "userjoins":
                            server.LogUserJoins = channel.Id;

                            break;
                        case "userleaves":
                            server.LogUserLeaves = channel.Id;

                            break;
                        case "bans":
                            server.LogBans = channel.Id;

                            break;
                        case "unbans":
                            server.LogUnbans = channel.Id;

                            break;
                        case "voicechannelconnections":
                            server.LogVoiceChannelConnections = channel.Id;

                            break;
                        case "levelannouncements":
                            server.LogLevelAnnouncements = channel.Id;

                            break;
                        case "fishlevels":
                            server.LogFishLevels = channel.Id;

                            break;
                        case "antiraids":
                            server.LogAntiraids = channel.Id;

                            break;
                        case "greetings":
                            server.LogGreetings = channel.Id;

                            break;
                        case "all":
                        {
                            if (server.IsPremium)
                                server.ModLog = channel.Id;

                            server.LogDeletedMessages = channel.Id;
                            server.LogUpdatedMessages = channel.Id;
                            server.LogFilteredPhrases = channel.Id;
                            server.LogUserJoins = channel.Id;
                            server.LogUserLeaves = channel.Id;
                            server.LogBans = channel.Id;
                            server.LogUnbans = channel.Id;
                            server.LogVoiceChannelConnections = channel.Id;
                            server.LogLevelAnnouncements = channel.Id;
                            server.LogFishLevels = channel.Id;
                            server.LogAntiraids = channel.Id;
                            server.LogGreetings = channel.Id;
                        }

                            break;
                        default:
                            logTypes.Remove(type);

                            break;
                    }
                }
                else
                {
                    await ConsoleLogger.LogAsync($"Server has disabled log type: [ID: {guildId} | Type: {type.ToUpperInvariant()}]", LogLvl.DEBUG);
                    switch (type.ToLower())
                    {
                        case "modlog":
                            server.ModLog = 0;

                            break;
                        case "deletedmessages":
                            server.LogDeletedMessages = 0;

                            break;
                        case "updatedmessages":
                            server.LogUpdatedMessages = 0;

                            break;
                        case "filteredphrases":
                            server.LogFilteredPhrases = 0;

                            break;
                        case "userjoins":
                            server.LogUserJoins = 0;

                            break;
                        case "userleaves":
                            server.LogUserLeaves = 0;

                            break;
                        case "bans":
                            server.LogBans = 0;

                            break;
                        case "unbans":
                            server.LogUnbans = 0;

                            break;
                        case "voicechannelconnections":
                            server.LogVoiceChannelConnections = 0;

                            break;
                        case "levelannouncements":
                            server.LogLevelAnnouncements = 0;

                            break;
                        case "fishlevels":
                            server.LogFishLevels = 0;

                            break;
                        case "antiraids":
                            server.LogAntiraids = 0;

                            break;
                        case "greetings":
                            server.LogGreetings = 0;

                            break;
                        case "all":
                        {
                            if (server.IsPremium)
                                server.ModLog = 0;

                            server.LogDeletedMessages = 0;
                            server.LogUpdatedMessages = 0;
                            server.LogFilteredPhrases = 0;
                            server.LogUserJoins = 0;
                            server.LogUserLeaves = 0;
                            server.LogBans = 0;
                            server.LogUnbans = 0;
                            server.LogVoiceChannelConnections = 0;
                            server.LogLevelAnnouncements = 0;
                            server.LogFishLevels = 0;
                            server.LogAntiraids = 0;
                            server.LogGreetings = 0;
                        }

                            break;
                        default:
                            logTypes.Remove(type);

                            break;
                    }
                }
            }

            await DatabaseQueries.UpdateAsync(server);

            return logTypes;
        }
    }
}