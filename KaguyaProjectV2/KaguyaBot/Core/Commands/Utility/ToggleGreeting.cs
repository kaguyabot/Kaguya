using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Utility
{
    public class ToggleGreeting : KaguyaBase
    {
        [UtilityCommand]
        [Command("ToggleGreeting")]
        [Alias("tg")]
        [Summary("Toggles this server's greeting message. If it is currently disabled, " +
                 "this command will enable it (and vice versa).")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.SendMessages)]
        public async Task Command()
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);

            server.CustomGreetingIsEnabled = server.CustomGreetingIsEnabled switch
            {
                true => false,
                false => true
            };

            if (server.CustomGreetingIsEnabled)
                await SendBasicSuccessEmbedAsync($"Successfully enabled this server's greeting message.");
            else
                await SendBasicSuccessEmbedAsync($"Successfully disabled this server's greeting message.");

            await DatabaseQueries.UpdateAsync(server);
        }
    }
}