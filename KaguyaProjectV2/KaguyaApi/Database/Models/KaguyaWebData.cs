using Humanizer;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using Newtonsoft.Json;

namespace KaguyaProjectV2.KaguyaApi.Database.Models
{
    public class KaguyaWebData
    {
        [JsonProperty(PropertyName = "registered_user_count")]
        public int RegisteredUserCount;
        [JsonProperty(PropertyName = "server_count")]
        public int ServerCount;
        [JsonProperty(PropertyName = "commands_used_alltime")]
        public int CommandsUsedAllTime;
        [JsonProperty(PropertyName = "currency_in_circulation")]
        public int CurrencyInCirculation;

        public KaguyaWebData()
        {
            RegisteredUserCount = DatabaseQueries.GetCountAsync<User>().Result;
            ServerCount = DatabaseQueries.GetCountAsync<Server>().Result;
            CommandsUsedAllTime = DatabaseQueries.GetCountAsync<CommandHistory>().Result;
            CurrencyInCirculation = DatabaseQueries.GetTotalCurrency();
        }
    }
}