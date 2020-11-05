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

    public class KaguyaDbSettings : ILinqToDBSettings
    {
        private readonly KaguyaApiConfig _apiConfig;
        public KaguyaDbSettings(KaguyaApiConfig apiConfig) { _apiConfig = apiConfig; }
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
                    ConnectionString = $"Server={_apiConfig.ServerIp};Port={_apiConfig.Port};" +
                                       $"Database={_apiConfig.SchemaName};Uid={_apiConfig.Username};" +
                                       $"Pwd={_apiConfig.Password};charset={_apiConfig.CharSet};"
                };
            }
        }
    }
}