using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Global;
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
                    ConnectionString = $"Server={GlobalProperties.MySQL_Server.Split(':').First()};Port={GlobalProperties.MySQL_Server.Split(':').Last()};" +
                    $"Database={GlobalProperties.MySQL_Database};Uid={GlobalProperties.MySQL_Username};Pwd={GlobalProperties.MySQL_Password};charset=utf8;",
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
