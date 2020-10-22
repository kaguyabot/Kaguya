using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Services;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class SetGame : KaguyaBase
    {
        [OwnerCommand]
        [Command("SetGame")]
        [Summary("Sets the game to the desired text and activity type. Default activity type is " +
                 "`Playing`\n\n" +
                 "__Activity Numbers:__\n" +
                 "- 0 = Playing\n" +
                 "- 1 = Streaming\n" +
                 "- 2 = Listening\n" +
                 "- 3 = Watching\n\n" +
                 "Specify a number to change the activity type.")]
        [Remarks("<text> [activity num]")]
        public async Task Command(string text, int num = 0)
        {
            if (num > 3 || num < 0)
            {
                await SendBasicErrorEmbedAsync("The activity number must be between 0 and 3. Your number " +
                                               "was " +
                                               num);

                return;
            }

            var type = (ActivityType) num;
            await GameRotationService.Set(text, type);

            await SendBasicSuccessEmbedAsync($"{Context.User.Mention} Succesfully set the game to:\n" +
                                             $"{type.Humanize(LetterCasing.Sentence)} {text}");
        }
    }
}