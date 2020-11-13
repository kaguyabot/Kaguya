using System;
using System.Threading.Tasks;
using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "filteredphrases")]
    public class FilteredPhrase : IKaguyaQueryable<FilteredPhrase>, IKaguyaUnique<FilteredPhrase>, IServerSearchable<FilteredPhrase>
    {
        [Column(Name = "ServerId")]
        [NotNull]
        public ulong ServerId { get; set; }

        [Column(Name = "Phrase")]
        [NotNull]
        public string Phrase { get; set; }

        public static event Func<FilteredPhraseEventArgs, Task> OnDetection;
        public static void Trigger(FilteredPhraseEventArgs fpArgs) => OnDetection?.Invoke(fpArgs);
        
        /// <summary>
        /// FK_KaguyaServer_FilteredPhrases
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "Id", CanBeNull = false)]
        public Server Server { get; set; }

        public bool Equals(FilteredPhrase other) => ServerId == other?.ServerId && Phrase == other.Phrase;
    }

    public class FilteredPhraseEventArgs : EventArgs
    {
        public Server Server;
        public string Phrase;
        public IUser Author => Message.Author;
        public IMessage Message;
        public FilteredPhraseEventArgs(Server server, string phrase, IMessage message)
        {
            Server = server;
            Phrase = phrase;
            Message = message;
        }
    }
}