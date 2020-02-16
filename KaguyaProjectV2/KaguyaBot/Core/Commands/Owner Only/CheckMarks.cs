using Discord;
using Discord.Commands;
using Discord.Rest;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class CheckMarks : KaguyaBase
    {
        /// <summary>
        /// Checks off items in the #todo-list chat channel.
        /// </summary>
        [OwnerCommand]
        [Command("check")]
        [Summary("Checks off items in the Kaguya Support #todo-list channel. IDs are separated by spaces.")]
        [Remarks("<ID> {...}\n651506875405434937 652020702387372044")]
        [RequireBotPermission(GuildPermission.AddReactions)]
        public async Task Check(params string[] ids)
        {
            var channel = ConfigProperties.Client.GetGuild(546880579057221644).GetTextChannel(546883647429410826);

            foreach (var element in ids)
            {
                if (ulong.TryParse(element, out ulong id))
                {
                    var msg = await channel.GetMessageAsync(id);
                    await ((RestUserMessage)msg).AddReactionAsync(new Emoji("✅"));
                }
            }

            var sentMsg = await ReplyAsync("Reactions added!");

            await Task.Delay(3000);

            await channel.DeleteMessageAsync(sentMsg);
            await channel.DeleteMessageAsync(Context.Message);
        }
    }
}
