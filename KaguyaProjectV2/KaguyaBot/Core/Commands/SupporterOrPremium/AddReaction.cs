using Centvrio.Emoji;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.SupporterOrPremium
{
    public class AddReaction : KaguyaBase
    {
        [SupporterCommand]
        [Command("React")]
        [Summary("Takes a line of text and message ID and adds a reaction to the message. If no message ID is specified, the command-invoking " +
            "message will be the recipient of the reactions.")]
        [Remarks("<text>\n<text> <message ID>")]
        [RequireBotPermission(GuildPermission.AddReactions)]
        public async Task React(string text, ulong msgId = 0)
        {
            IMessage message = null;
            if (msgId == 0)
                message = Context.Message;
            if (msgId != 0)
                message = await Context.Channel.GetMessageAsync(msgId);

            if (text.Length > 10)
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

            foreach (char letter in text)
            {
                emojis.Add(new Emoji($"{ReturnEmoji(letter)}"));
            }

            await (message as IUserMessage).AddReactionsAsync(emojis.ToArray());
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
