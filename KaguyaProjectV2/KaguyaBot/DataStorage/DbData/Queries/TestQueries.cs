using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context;
using System;
using System.Data;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries
{
    public static class TestQueries
    {
        public static async Task<bool> TestConnection()
        {
            using (var db = new KaguyaDb())
            {
                try
                {
                    return db.Connection.State.Equals(ConnectionState.Open);
                }
                catch(Exception e)
                {
                    await ConsoleLogger.LogAsync($"Failed to establish database connection!! {e.Message}", LogLvl.ERROR);
                    return false;
                }
            }
        }
    }
}
