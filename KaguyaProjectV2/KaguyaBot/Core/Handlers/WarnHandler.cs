using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Commands.Administration;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    public static class WarnHandler
    {
        public static async Task OnWarn(WarnHandlerEventArgs args)
        {
            var currentSettings = await DatabaseQueries.GetFirstForServerAsync<WarnSetting>(args.Server.ServerId);

            if (currentSettings == null)
                return;

            List<WarnedUser> currentWarnings =
                await DatabaseQueries.GetAllForServerAndUserAsync<WarnedUser>(args.WarnedUser.UserId, args.Server.ServerId);

            int warnCount = currentWarnings.Count;

            SocketGuildUser guildUser = ConfigProperties.Client.GetGuild(args.WarnedUser.ServerId).GetUser(args.WarnedUser.UserId);

            int? muteNum = currentSettings.Mute;
            int? kickNum = currentSettings.Kick;
            int? shadowbanNum = currentSettings.Shadowban;
            int? banNum = currentSettings.Ban;

            if (warnCount == banNum)
            {
                var ban = new Ban();

                try
                {
                    await ban.AutoBanUserAsync(guildUser, "User has been automatically banned due to " +
                                                          $"reaching the specified warning threshold for bans " +
                                                          $"({warnCount} warnings).");

                    await ConsoleLogger.LogAsync($"User [{guildUser} | {guildUser.Id}] has been " +
                                                 $"automatically banned in guild " +
                                                 $"[{guildUser.Guild} | {guildUser.Guild.Id}]", LogLvl.DEBUG);
                }
                catch (Exception e)
                {
                    await ConsoleLogger.LogAsync($"An attempt was made to auto-ban a user " +
                                                 $"who hit the warn threshold for this event " +
                                                 $"to take place. This failed due to an exception.\n" +
                                                 $"Exception Message: {e.Message}\n" +
                                                 $"Inner Exception Message: {e.InnerException?.Message}\n" +
                                                 $"Guild: {args.Server.ServerId}", LogLvl.WARN);
                }

                return;
            }

            if (warnCount == shadowbanNum)
            {
                var shadowban = new Shadowban();

                try
                {
                    await shadowban.AutoShadowbanUserAsync(guildUser);
                    await ConsoleLogger.LogAsync($"User [{guildUser} | {guildUser.Id}] has been " +
                                                 $"automatically shadowbanned in guild " +
                                                 $"[{guildUser.Guild} | {guildUser.Guild.Id}]", LogLvl.DEBUG);
                }
                catch (Exception e)
                {
                    await ConsoleLogger.LogAsync($"An attempt was made to auto-shadowban a user " +
                                                 $"who hit the warn threshold for this event " +
                                                 $"to take place. This failed due to an exception.\n" +
                                                 $"Exception Message: {e.Message}\n" +
                                                 $"Inner Exception Message: {e.InnerException?.Message}\n" +
                                                 $"Guild: {args.Server.ServerId}", LogLvl.WARN);
                }

                return;
            }

            if (warnCount == kickNum)
            {
                var kick = new Kick();

                try
                {
                    await kick.AutoKickUserAsync(guildUser, $"User has been automatically kicked due to " +
                                                            $"reaching the specified warning threshold for kicks " +
                                                            $"({warnCount} warnings).");

                    await ConsoleLogger.LogAsync($"User [{guildUser} | {guildUser.Id}] has been " +
                                                 $"automatically kicked in guild " +
                                                 $"[{guildUser.Guild} | {guildUser.Guild.Id}]", LogLvl.DEBUG);
                }
                catch (Exception e)
                {
                    await ConsoleLogger.LogAsync($"An attempt was made to auto-kick a user " +
                                                 $"who hit the warn threshold for this event " +
                                                 $"to take place. This failed due to an exception.\n" +
                                                 $"Exception Message: {e.Message}\n" +
                                                 $"Inner Exception Message: {e.InnerException?.Message}\n" +
                                                 $"Guild: {args.Server.ServerId}", LogLvl.WARN);
                }

                return;
            }

            if (warnCount == muteNum)
            {
                var mute = new Mute();
                
                try
                {
                    await mute.AutoMute(guildUser);
                    await ConsoleLogger.LogAsync($"User [{guildUser} | {guildUser.Id}] has been " +
                                                 $"automatically muted in guild " +
                                                 $"[{guildUser.Guild} | {guildUser.Guild.Id}]", LogLvl.DEBUG);
                }
                catch (Exception e)
                {
                    await ConsoleLogger.LogAsync($"An attempt was made to auto-mute a user " +
                                                 $"who hit the warn threshold for this event " +
                                                 $"to take place. This failed due to an exception.\n" +
                                                 $"Exception Message: {e.Message}\n" +
                                                 $"Inner Exception Message: {e.InnerException?.Message}\n" +
                                                 $"Guild: {args.Server.ServerId}", LogLvl.WARN);
                }
            }
        }
    }
}