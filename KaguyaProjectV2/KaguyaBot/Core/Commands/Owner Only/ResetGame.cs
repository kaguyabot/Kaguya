using System.Threading.Tasks;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Services;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class ResetGame : KaguyaBase
    {
        [OwnerCommand]
        [Command("ResetGame")]
        [Summary("Clears the currently active livestream, resumes the game rotation service, " +
                 "and sets the currently playing game back to the default value.")]
        [Remarks("")]
        public async Task Command()
        {
            GameRotationService.Resume();
            await GameRotationService.SetToDefault();
            await SendBasicSuccessEmbedAsync($"{Context.User.Mention} successfully re-enabled the game rotation service " +
                                             $"and set the currently playing game back to the default value.");
        }
    }
}