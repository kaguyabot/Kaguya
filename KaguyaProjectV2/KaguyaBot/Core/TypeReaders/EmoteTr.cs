using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Helpers;

namespace KaguyaProjectV2.KaguyaBot.Core.TypeReaders
{
    public class EmoteTr : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            // a pepeLaughing 588362912494780417
            var emoteStr = input
                .Replace("<", "")
                .Replace(">", "");
            var emoteSplits = emoteStr.Split(':');
            
            bool gifEmote = emoteStr.StartsWith("a:");

            bool validEmoteId = ulong.TryParse(emoteSplits.LastOrDefault(), out ulong emoteId);
            if (!validEmoteId)
            {
                return TypeReaderResult.FromError(CommandError.Exception, 
                    "Could not parse this Emote's ID from the provided input.");
            }

            string emoteName = gifEmote ? emoteSplits[1] : emoteSplits[0];
            
            Emote result = DiscordHelpers.CreateInstance<Emote>(emoteId, emoteName, gifEmote);
            if (context.Guild.Emotes.FirstOrDefault(x => x.Id == emoteId) == null)
            {
                return TypeReaderResult.FromError(CommandError.ObjectNotFound, "The current guild does not contain this emote.");
            }
            
            return TypeReaderResult.FromSuccess(result);
        }
    }
}