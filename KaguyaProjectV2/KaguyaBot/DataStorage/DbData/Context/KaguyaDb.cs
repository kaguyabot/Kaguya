using LinqToDB;
using System.Data;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context
{
    public partial class KaguyaDb : LinqToDB.Data.DataConnection
    {
        public KaguyaDb() : base("KaguyaContext") { }
        public ITable<User> Users => GetTable<User>();
        public ITable<Server> Servers => GetTable<Server>();
    }
}
