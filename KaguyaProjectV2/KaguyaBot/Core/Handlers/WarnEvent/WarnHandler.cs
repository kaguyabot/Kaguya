using KaguyaProjectV2.KaguyaBot.Core.Commands.Administration;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.WarnEvent
{
    public static class WarnHandler
    {
        public static async void OnWarn(object warn, WarnHandlerEventArgs args)
        {
            var currentSettings = await DatabaseQueries.GetWarnConfigForServerAsync(args.server.ServerId);
            var currentWarnings = await DatabaseQueries.GetWarningsForUserAsync(args.server.ServerId, args.warnedUser.UserId);
            var warnCount = currentWarnings.Count;

            var guildUser = ConfigProperties.Client.GetGuild(args.warnedUser.ServerId).GetUser(args.warnedUser.UserId);
            var kaguya = ConfigProperties.Client.CurrentUser;

            int muteNum = currentSettings.Mute;
            int kickNum = currentSettings.Kick;
            int shadowbanNum = currentSettings.Shadowban;
            int banNum = currentSettings.Ban;

            if (warnCount == banNum)
            {
                var ban = new Ban();
                var modLog = new PremiumModerationLog
                {
                    Server = args.server,
                    Moderator = kaguya,
                    ActionRecipient = guildUser,
                    Action = PremiumModActionHandler.AUTOBAN,
                    Reason = $"Automatic ban due to the user reaching {warnCount} warnings."
                };
                await ban.AutoBanUserAsync(guildUser, "User has been automatically banned due to " + 
                                                      $"reaching the specified warning threshold for bans " +
                                                      $"({warnCount} warnings).");
                await PremiumModerationLog.SendModerationLog(modLog);
                await ConsoleLogger.LogAsync($"User [{guildUser} | {guildUser.Id}] has been " +
                                        $"automatically banned in guild " +
                                        $"[{guildUser.Guild} | {guildUser.Guild.Id}]", LogLvl.DEBUG);
                return;
            }

            if (warnCount == shadowbanNum)
            {
                var shadowban = new Shadowban();
                var modLog = new PremiumModerationLog
                {
                    Server = args.server,
                    Moderator = kaguya,
                    ActionRecipient = guildUser,
                    Action = PremiumModActionHandler.AUTOSHADOWBAN,
                    Reason = $"Automatic shadowban due to the user reaching {warnCount} warnings."
                };
                await shadowban.AutoShadowbanUserAsync(guildUser);
                await PremiumModerationLog.SendModerationLog(modLog);
                await ConsoleLogger.LogAsync($"User [{guildUser} | {guildUser.Id}] has been " +
                                        $"automatically shadowbanned in guild " +
                                        $"[{guildUser.Guild} | {guildUser.Guild.Id}]", LogLvl.DEBUG);
                return;
            }

            if (warnCount == kickNum)
            {
                var kick = new Kick();
                var modLog = new PremiumModerationLog
                {
                    Server = args.server,
                    Moderator = kaguya,
                    ActionRecipient = guildUser,
                    Action = PremiumModActionHandler.AUTOKICK,
                    Reason = $"Automatic kick due to the user reaching {warnCount} warnings."
                };
                await kick.AutoKickUserAsync(guildUser, $"User has been automatically kicked due to " +
                                                        $"reaching the specified warning threshold for kicks " +
                                                        $"({warnCount} warnings).");
                await PremiumModerationLog.SendModerationLog(modLog);
                await ConsoleLogger.LogAsync($"User [{guildUser} | {guildUser.Id}] has been " +
                                        $"automatically kicked in guild " +
                                        $"[{guildUser.Guild} | {guildUser.Guild.Id}]", LogLvl.DEBUG);
                return;
            }

            if (warnCount == muteNum)
            {
                var mute = new Mute();
                var modLog = new PremiumModerationLog
                {
                    Server = args.server,
                    Moderator = kaguya,
                    ActionRecipient = guildUser,
                    Action = PremiumModActionHandler.AUTOMUTE,
                    Reason = $"Automatic mute due to the user reaching {warnCount} warnings."
                };
                await mute.AutoMute(guildUser);
                await PremiumModerationLog.SendModerationLog(modLog);
                await ConsoleLogger.LogAsync($"User [{guildUser} | {guildUser.Id}] has been " +
                                        $"automatically muted in guild " +
                                        $"[{guildUser.Guild} | {guildUser.Guild.Id}]", LogLvl.DEBUG);
            }
        }
    }
}
