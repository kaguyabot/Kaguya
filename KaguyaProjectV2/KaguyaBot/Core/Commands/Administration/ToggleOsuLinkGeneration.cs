using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class ToggleOsuLinkGeneration : KaguyaBase
    {
        [UtilityCommand]
        [Command("ToggleOsuLinks")]
        [Alias("tol")]
        [Summary("Allows a server administrator to toggle the automatic osu! beatmap link parsing feature. " +
                 "The osu! beatmap link parser will automatically return data for the beatmap that was linked " +
                 "in chat if Kaguya detects it. Disabling this is recommended if you already have a more preferable " +
                 "bot that does the same thing, or if you simply don't care for the feature.")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public async Task Command()
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            string desc = (server.OsuLinkParsingEnabled
                ? "Successfully disabled the automatic osu! beatmap link parsing"
                : "Successfully enabled the automatic osu! beatmap link parsing");

            server.OsuLinkParsingEnabled = !server.OsuLinkParsingEnabled;

            var embed = new KaguyaEmbedBuilder
            {
                Title = "osu! Link Parsing Config",
                Description = desc,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Use this command again to {(server.OsuLinkParsingEnabled ? "disable" : "enable")}."
                }
            };
            await SendEmbedAsync(embed);
            await DatabaseQueries.UpdateAsync(server);
        }
    }
}
