using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Utility
{
    public class CreateTextChannel : KaguyaBase
    {
        [UtilityCommand]
        [Command("CreateTextChannel")]
        [Alias("ctc")]
        [Summary("Creates a standard text channel.")]
        [Remarks("<name>")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        public async Task Command([Remainder] string name)
        {
            await Context.Guild.CreateTextChannelAsync(name);
            await SendBasicSuccessEmbedAsync($"Successfully created a new text channel named `{name}`.");
        }
    }
}