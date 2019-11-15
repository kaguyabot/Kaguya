using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.Application.ApplicationStart;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Supporter
{
    public class AddReaction : ModuleBase<ShardedCommandContext>
    {
        [RequireSupporter]
        [Command("React")]
        [Summary("Takes a line of text, converts the letters/numbers into reactions, and then adds it to either " +
            "the most recent message in chat, or the specified message. Messages must be specified by ID.")]
        [Remarks("<text>\n<message ID> <text>")]
        [RequireBotPermission(GuildPermission.AddReactions)]
        public async Task React([Remainder]string text)
        {
            Emoji[] emojis = new Emoji[text.Length];
            foreach(char letter in text)
            {
                Emoji emoji = new Emoji(""); //CONTINUE HERE
            }
        }
    }
}
