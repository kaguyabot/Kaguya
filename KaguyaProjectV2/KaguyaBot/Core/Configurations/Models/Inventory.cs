using System.Collections.Generic;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Configurations.Models
{
    public class Inventory
    {
        public Inventory(ulong userId)
        {
            Consumables = DatabaseQueries.GetAllForUserAsync<UserConsumable>(userId).Result;
        }
        
        public IEnumerable<UserConsumable> Consumables { get; }
    }
}