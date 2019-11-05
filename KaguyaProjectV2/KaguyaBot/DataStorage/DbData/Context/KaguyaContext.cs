using LinqToDB.Configuration;
using LinqToDB.Data;
using System.Collections.Generic;
using System.Linq;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context
{
    public class KaguyaContext : IConnectionStringSettings
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public string ProviderName { get; set; }
        public bool IsGlobal => false;
    }

    public class KaguyaSettings : ILinqToDBSettings
    {
        public IEnumerable<IDataProviderSettings> DataProviders => Enumerable.Empty<IDataProviderSettings>();

        public string DefaultConfiguration => "KaguyaContext";
        public string DefaultDataProvider => "MySQL";

        public IEnumerable<IConnectionStringSettings> ConnectionStrings
        {
            get
            {
                yield return new KaguyaContext
                {
                    Name = "KaguyaContext",
                    ConnectionString = "Generate connection string from config here!",
                    ProviderName = "MySql.Data.MySqlClient"
                };
            }
        }
    }

    public class Init
    {
        public Init()
        {
            DataConnection.DefaultSettings = new KaguyaSettings();
        }
    }
}
