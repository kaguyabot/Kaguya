using KaguyaProjectV2.KaguyaApi.Database.Models;
using LinqToDB;
using LinqToDB.Data;

namespace KaguyaProjectV2.KaguyaApi.Database.Context
{
    public partial class KaguyaDb : DataConnection
    {
        public KaguyaDb() : base("KaguyaContext") { }
        public ITable<TopGgWebhook> TopGGUpvotes => GetTable<TopGgWebhook>();
    }
}
