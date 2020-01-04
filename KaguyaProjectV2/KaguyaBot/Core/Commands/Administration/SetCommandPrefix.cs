using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class SetCommandPrefix : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("SetPrefix")]
        [Alias("prefix", "sp")]
        [Summary("Changes the command prefix to the specified text. Limited to 5 characters. Use with no arguments to reset the prefix to `$`.")]
        [Remarks("k!\n$%\n<prefix>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetPrefix(string prefix)
        {
            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

            if(prefix.Length > 5)
            {
                await ConsoleLogger.LogAsync("Command prefix was too long. Not set.", DataStorage.JsonStorage.LogLvl.DEBUG);

                embed.WithDescription("Your command prefix may not be longer than 5 characters.");
                embed.SetColor(EmbedColor.RED);
                await ReplyAsync(embed: embed.Build());
                return;
            }

            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            server.CommandPrefix = prefix;
            await DatabaseQueries.UpdateServerAsync(server);

            embed.WithDescription($"Command prefix has been changed to `{prefix}`.");
            embed.WithFooter($"Use this command again without specifying a prefix to reset it.");
            embed.SetColor(EmbedColor.VIOLET);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("SetPrefix")]
        [Alias("prefix", "sp")]
        [Summary("Changes the command prefix to the specified text. Limited to 5 characters. Use with no arguments to reset the prefix to `$`.")]
        [Remarks("k!\n$%\n<prefix>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetPrefix()
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            server.CommandPrefix = "$";
            await DatabaseQueries.UpdateServerAsync(server);

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder 
            {
                Description = "Reset the prefix back to `$`."
            };
            embed.SetColor(EmbedColor.VIOLET);

            await ReplyAsync(embed: embed.Build());
        }
    }
}