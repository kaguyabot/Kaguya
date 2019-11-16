using Discord;
using Discord.Commands;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.Log;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class SetCommandPrefix : ModuleBase<ShardedCommandContext>
    {
        [Command("SetPrefix")]
        [Alias("prefix", "sp")]
        [Summary("Changes the command prefix to the specified text. Limited to 5 characters. Use with no arguments to reset the prefix to `$`.")]
        [Remarks("k!\n$%\n")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetPrefix(string prefix)
        {
            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

            if(prefix.Length > 5)
            {
                await ConsoleLogger.Log("Command prefix was too long. Not set.", DataStorage.JsonStorage.LogLevel.DEBUG);

                embed.WithDescription("Your command prefix may not be longer than 5 characters.");
                embed.SetColor(EmbedColor.RED);
                await ReplyAsync(embed: embed.Build());
                return;
            }

            Server server = ServerQueries.GetServer(Context.Guild.Id);
            server.CommandPrefix = prefix;
            ServerQueries.UpdateServer(server);

            embed.WithDescription($"Command prefix has been changed to `{prefix}`.");
            embed.WithFooter($"Use this command again without specifying a prefix to reset it.");
            embed.SetColor(EmbedColor.VIOLET);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("setprefix")]
        [Alias("prefix", "sp")]
        [Summary("Changes the command prefix to the specified text. Limited to 5 characters. Use with no arguments to reset the prefix to `$`.")]
        [Remarks("setprefix k! \nsp 12345 \nprefix")]
        public async Task SetPrefix()
        {
            Server server = ServerQueries.GetServer(Context.Guild.Id);
            server.CommandPrefix = "$";
            ServerQueries.UpdateServer(server);

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder 
            {
                Description = "Reset the prefix back to `$`."
            };
            embed.SetColor(EmbedColor.VIOLET);

            await ReplyAsync(embed: embed.Build());
        }
    }
}