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
using Centvrio.Emoji;
using System.Reflection;
using Discord.WebSocket;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Supporter
{
    public class AddReaction : InteractiveBase<ShardedCommandContext>
    {
        [RequireSupporter]
        [Command("React")]
        [Summary("Takes a line of text, converts the letters/numbers into reactions, and then adds it to either " +
            "the most recent message in chat, or the specified message. Messages must be specified by ID.")]
        [Remarks("<text>\n<message ID> <text>")]
        [RequireBotPermission(GuildPermission.AddReactions)]
        public async Task React([Remainder]string text)
        {
            if(text.Length > 10)
            {
                KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
                {
                    Description = "Your reaction may not be more than 10 characters long."
                };
                embed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: embed.Build());
                return;
            }
                
            text.Replace(" ", "");
            List<Emoji> emojis = new List<Emoji>();

            foreach(char letter in text)
            {
                emojis.Add(new Emoji($"{ReturnEmoji(letter)}"));
            }

            await Context.Message.AddReactionsAsync(emojis.ToArray());
        }

        private static UnicodeString ReturnEmoji(char letter)
        {
            switch (letter.ToString().ToUpper())
            {
                case "A": return RegionalIndicator.A;
                case "B": return RegionalIndicator.B;
                case "C": return RegionalIndicator.C;
                case "D": return RegionalIndicator.D;
                case "E": return RegionalIndicator.E;
                case "F": return RegionalIndicator.F;
                case "G": return RegionalIndicator.G;
                case "H": return RegionalIndicator.H;
                case "I": return RegionalIndicator.I;
                case "J": return RegionalIndicator.J;
                case "K": return RegionalIndicator.K;
                case "L": return RegionalIndicator.L;
                case "M": return RegionalIndicator.M;
                case "N": return RegionalIndicator.N;
                case "O": return RegionalIndicator.O;
                case "P": return RegionalIndicator.P;
                case "Q": return RegionalIndicator.Q;
                case "R": return RegionalIndicator.R;
                case "S": return RegionalIndicator.S;
                case "T": return RegionalIndicator.T;
                case "U": return RegionalIndicator.U;
                case "V": return RegionalIndicator.V;
                case "W": return RegionalIndicator.W;
                case "X": return RegionalIndicator.X;
                case "Y": return RegionalIndicator.Y;
                case "Z": return RegionalIndicator.Z;
                default: return WarningSign.Warning;
            }
        }
    }
}
