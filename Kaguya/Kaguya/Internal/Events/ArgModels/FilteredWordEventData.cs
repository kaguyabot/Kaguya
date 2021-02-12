using Discord;

namespace Kaguya.Internal.Events.ArgModels
{
    public class FilteredWordEventData
    {
        public ulong ServerId { get; set; }
        public ulong UserId { get; set; }
        public string Phrase { get; set; }
        public IMessage Message { get; set; }
    }
}