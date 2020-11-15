﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration.LogCommands
{
    public static class LogQuery
    {
        /// <summary>
        ///     Performs all necessary actions that allow a server to have their logtypes enabled or disabled.
        /// </summary>
        /// <param name="args">A period separated list of logtypes to enable/disable.</param>
        /// <param name="guildId"></param>
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
                        case "warns":
                            ThrowExIfNotPremium(server);
                            server.LogWarns = channel.Id;

                            break;
                        case "unwarns":
                            ThrowExIfNotPremium(server);
                            server.LogUnwarns = channel.Id;

                            break;
                        case "shadowbans":
                            ThrowExIfNotPremium(server);
                            server.LogShadowbans = channel.Id;

                            break;
                        case "unshadowbans":
                            ThrowExIfNotPremium(server);
                            server.LogUnshadowbans = channel.Id;

                            break;
                        case "mutes":
                            ThrowExIfNotPremium(server);
                            server.LogMutes = channel.Id;

                            break;
                        case "unmutes":
                            ThrowExIfNotPremium(server);
                            server.LogUnmutes = channel.Id;

                            break;
                        case "all":
                        {
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
                            if (server.IsPremium)
                            {
                                server.LogWarns = channel.Id;
                                server.LogUnwarns = channel.Id;
                                server.LogShadowbans = channel.Id;
                                server.LogUnshadowbans = channel.Id;
                                server.LogMutes = channel.Id;
                                server.LogUnmutes = channel.Id;
                            }
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
                        case "warns":
                            ThrowExIfNotPremium(server);
                            server.LogWarns = 0;

                            break;
                        case "unwarns":
                            ThrowExIfNotPremium(server);
                            server.LogUnwarns = 0;

                            break;
                        case "shadowbans":
                            ThrowExIfNotPremium(server);
                            server.LogShadowbans = 0;

                            break;
                        case "unshadowbans":
                            ThrowExIfNotPremium(server);
                            server.LogUnshadowbans = 0;

                            break;
                        case "mutes":
                            ThrowExIfNotPremium(server);
                            server.LogMutes = 0;

                            break;
                        case "unmutes":
                            ThrowExIfNotPremium(server);
                            server.LogUnmutes = 0;

                            break;
                        case "all":
                        {
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
                            if (server.IsPremium)
                            {
                                server.LogWarns = 0;
                                server.LogUnwarns = 0;
                                server.LogShadowbans = 0;
                                server.LogUnshadowbans = 0;
                                server.LogMutes = 0;
                                server.LogUnmutes = 0;
                            }
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

        /// <summary>
        /// Throws a <see cref="KaguyaPremiumException"/> if the server is not premium.
        /// </summary>
        /// <param name="server"></param>
        private static void ThrowExIfNotPremium(Server server)
        {
            if (!server.IsPremium)
                throw new KaguyaPremiumException();
        }
    }
}