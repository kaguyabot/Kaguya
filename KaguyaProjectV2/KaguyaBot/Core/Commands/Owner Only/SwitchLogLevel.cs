using Discord;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class SwitchLogLevel : KaguyaBase
    {
        [OwnerCommand]
        [Command("swaploglevel")]
        [Alias("swaplog", "sll")]
        [Summary("Switches the current console log severity to the specified severity. " +
                 "Valid severities are `Trace, Debug, Info, Warn, Error`")]
        [Remarks("<severity>")]
        public async Task SwapLogLevel(string level)
        {
            LogLvl curLog = ConfigProperties.LogLevel;
            string validSeverities = "Trace, Debug, Info, Warn, Error";

            ConfigProperties.LogLevel = level.ToLower() switch
            {
                "trace" => LogLvl.TRACE,
                "debug" => LogLvl.DEBUG,
                "info" => LogLvl.INFO,
                "warn" => LogLvl.WARN,
                "error" => LogLvl.ERROR,
                _ => throw new ArgumentOutOfRangeException($"Valid logtypes are `{validSeverities}`", new Exception())
            };

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully changed LogLevel from `{curLog.Humanize()}` to `{ConfigProperties.LogLevel.Humanize()}`",
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Note: This loglevel will return back to `{curLog.Humanize()}` after a restart."
                }
            };

            embed.SetColor(EmbedColor.VIOLET);

            await ReplyAsync(embed: embed.Build());
        }
    }
}