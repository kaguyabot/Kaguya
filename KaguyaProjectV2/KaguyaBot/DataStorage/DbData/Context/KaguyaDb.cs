using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using LinqToDB;
using LinqToDB.Data;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context
{
    public partial class KaguyaDb : DataConnection
    {
        public KaguyaDb() : base("KaguyaContext") { }
        public ITable<AntiRaidConfig> AntiRaid => GetTable<AntiRaidConfig>();
        public ITable<AutoAssignedRole> AutoAssignedRoles => GetTable<AutoAssignedRole>();
        public ITable<BlackListedChannel> BlackListedChannels => GetTable<BlackListedChannel>();
        public ITable<CommandHistory> CommandHistories => GetTable<CommandHistory>();
        public ITable<EightBall> EightBall => GetTable<EightBall>();
        public ITable<FilteredPhrase> FilteredPhrases => GetTable<FilteredPhrase>();
        public ITable<Fish> Fish => GetTable<Fish>();
        public ITable<GambleHistory> GambleHistories => GetTable<GambleHistory>();
        public ITable<MutedUser> MutedUsers => GetTable<MutedUser>();
        public ITable<PremiumKey> PremiumKeys => GetTable<PremiumKey>();
        public ITable<Quote> Quotes => GetTable<Quote>();
        public ITable<Reminder> Reminders => GetTable<Reminder>();
        public ITable<Praise> Praise => GetTable<Praise>();
        public ITable<Rep> Rep => GetTable<Rep>();
        public ITable<Server> Servers => GetTable<Server>();
        public ITable<ServerExp> ServerExp => GetTable<ServerExp>();
        public ITable<User> Users => GetTable<User>();
        public ITable<UserConsumable> UserConsumables => GetTable<UserConsumable>();
        public ITable<WarnSetting> WarnActions => GetTable<WarnSetting>();
        public ITable<WarnedUser> WarnedUsers => GetTable<WarnedUser>();
    }
}