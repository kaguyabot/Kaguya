using KaguyaProjectV2.KaguyaBot.Core.Commands.Administration;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.WarnEvent
{
    public static class WarnHandler
    {
        public static async void OnWarn(object warn, WarnHandlerEventArgs args)
        {
            var currentSettings = await ServerQueries.GetWarnConfigForServerAsync(args.server.Id);
            var currentWarnings = await ServerQueries.GetWarningsForUserAsync(args.server.Id, args.warnedUser.UserId);
            var warnCount = currentWarnings.Count;

            var guildUser = ConfigProperties.client.GetGuild(args.warnedUser.ServerId).GetUser(args.warnedUser.UserId);
            var kaguya = ConfigProperties.client.GetGuild(args.warnedUser.ServerId).GetUser(538910393918160916);

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
                await ban.BanUser(guildUser);
                await PremiumModerationLog.SendModerationLog(modLog);
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
                await shadowban.ShadowbanUser(guildUser);
                await PremiumModerationLog.SendModerationLog(modLog);
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
                await kick.KickUser(guildUser);
                await PremiumModerationLog.SendModerationLog(modLog);
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
            }
        }
    }
}
