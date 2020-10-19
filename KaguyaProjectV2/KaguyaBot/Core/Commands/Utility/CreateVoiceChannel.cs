using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Utility
{
    public class CreateVoiceChannel : KaguyaBase
    {
        [UtilityCommand]
        [Command("CreateVoiceChannel")]
        [Alias("cvc")]
        [Summary("Creates a standard voice channel.")]
        [Remarks("<name>")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        public async Task Command([Remainder] string name)
        {
            await Context.Guild.CreateVoiceChannelAsync(name);
            await SendBasicSuccessEmbedAsync($"Successfully created a new voice channel named " +
                                             $"`{name}`.");
        }
    }
}