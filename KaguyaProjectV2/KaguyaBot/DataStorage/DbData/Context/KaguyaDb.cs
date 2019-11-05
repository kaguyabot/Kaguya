using LinqToDB;
using System.Data;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context
{
    public partial class KaguyaDb : LinqToDB.Data.DataConnection
    {
        public KaguyaDb() : base("KaguyaContext") { }
        //public ITable<-`Model of Table`-> -`Name of List`- => GetTable<-`Model of Table`->();
    }
}
