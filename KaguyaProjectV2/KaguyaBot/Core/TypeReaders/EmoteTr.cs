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
            string emoteStr = input
                              .Replace("<", "")
                              .Replace(">", "");

            string[] emoteSplits = emoteStr.Split(':');

            bool gifEmote = emoteStr.StartsWith("a:");

            bool validEmoteId = ulong.TryParse(emoteSplits.LastOrDefault(), out ulong emoteId);
            if (!validEmoteId)
            {
                return TypeReaderResult.FromError(CommandError.Exception,
                    "Could not parse this Emote's ID from the provided input.");
            }

            string emoteName = emoteSplits[1];

            var result = DiscordHelpers.CreateInstance<Emote>(emoteId, emoteName, gifEmote);
            if (context.Guild.Emotes.FirstOrDefault(x => x.Id == emoteId) == null)
            {
                // If a user copy/pastes a msg with an emote in it instead of typing the emote, look for this emote anyway.
                GuildEmote emoteAltPossibility = context.Guild.Emotes.FirstOrDefault(x => x.Name == emoteName);

                if (emoteAltPossibility == null)
                    return TypeReaderResult.FromError(CommandError.ObjectNotFound, "The current guild does not contain this emote.");

                return TypeReaderResult.FromSuccess(emoteAltPossibility);
            }

            return TypeReaderResult.FromSuccess(result);
        }
    }
}