using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class CheckMarks : ModuleBase<ShardedCommandContext>
    {
        /// <summary>
        /// Checks off items in the #todo-list chat channel.
        /// </summary>
        [OwnerCommand]
        [Command("check")]
        [Summary("Checks off items in the Kaguya Support #todo-list channel.")]
        [Remarks("<ID>.<ID> {...}\n651506875405434937.652020702387372044")]
        public async Task Check(string idString)
        {
            var ids = ArrayInterpreter.ReturnParams(idString);
            var channel = ConfigProperties.client.GetGuild(546880579057221644).GetTextChannel(546883647429410826);

            foreach (var element in ids)
            {
                if (ulong.TryParse(element, out ulong id))
                {
                    var msg = await channel.GetMessageAsync(id);
                    await ((RestUserMessage) msg).AddReactionAsync(new Emoji("✅"));
                }
            }

            var sentMsg = await ReplyAsync("Reactions added!");

            await Task.Delay(3000);

            await channel.DeleteMessageAsync(sentMsg);
            await channel.DeleteMessageAsync(Context.Message);
        }
    }
}
