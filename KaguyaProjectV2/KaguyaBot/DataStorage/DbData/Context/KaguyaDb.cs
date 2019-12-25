using LinqToDB;
using System.Data;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context
{
    public partial class KaguyaDb : LinqToDB.Data.DataConnection
    {
        public KaguyaDb() : base("KaguyaContext") { }
        public ITable<AutoAssignedRole> AutoAssignedRoles => GetTable<AutoAssignedRole>();
        public ITable<BlackListedChannel> BlackListedChannels => GetTable<BlackListedChannel>();
        public ITable<CommandHistory> CommandHistories => GetTable<CommandHistory>();
        public ITable<FilteredPhrase> FilteredPhrases => GetTable<FilteredPhrase>();
        public ITable<GambleHistory> GambleHistories => GetTable<GambleHistory>();
        public ITable<MutedUser> MutedUsers => GetTable<MutedUser>();
        public ITable<PremiumKey> PremiumKeys => GetTable<PremiumKey>();
        public ITable<Reminder> Reminders => GetTable<Reminder>();
        public ITable<Praise> Praise => GetTable<Praise>();
        public ITable<Rep> Rep => GetTable<Rep>();
        public ITable<Server> Servers => GetTable<Server>();
        public ITable<ServerExp> ServerExp => GetTable<ServerExp>();
        public ITable<SupporterKey> SupporterKeys => GetTable<SupporterKey>();
        public ITable<TwitchChannel> TwitchChannels => GetTable<TwitchChannel>();
        public ITable<User> Users => GetTable<User>();
        public ITable<WarnSetting> WarnActions => GetTable<WarnSetting>();
        public ITable<WarnedUser> WarnedUsers => GetTable<WarnedUser>();
    }
}