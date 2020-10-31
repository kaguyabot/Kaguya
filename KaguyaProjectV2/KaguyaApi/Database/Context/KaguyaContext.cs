using System.Collections.Generic;
using System.Linq;
using LinqToDB.Configuration;
using Microsoft.Extensions.Options;

namespace KaguyaProjectV2.KaguyaApi.Database.Context
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
        private readonly DbConfig _dbConfig;
        public KaguyaSettings(IOptions<DbConfig> dbConfig) { _dbConfig = dbConfig.Value; }
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
                    ProviderName = "MySql.Data.MySqlClient",
                    ConnectionString = $"Server={_dbConfig.ServerIp};Port={_dbConfig.Port};" +
                                       $"Database={_dbConfig.SchemaName};Uid={_dbConfig.Username};Pwd={_dbConfig.Password};charset={_dbConfig.CharSet};"
                };
            }
        }
    }
}