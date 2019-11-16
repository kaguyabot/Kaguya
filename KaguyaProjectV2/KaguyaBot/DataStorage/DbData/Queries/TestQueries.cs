using KaguyaProjectV2.KaguyaBot.Core.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context;
using System;
using System.Data;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries
{
    public static class TestQueries
    {
        public static bool TestConnection()
        {
            using (var db = new KaguyaDb())
            {
                try
                {
                    return db.Connection.State.Equals(ConnectionState.Open);
                }
                catch(Exception e)
                {
                    ConsoleLogger.Log($"Failed to establish database connection!! {e.Message}", Core.DataStorage.JsonStorage.LogLevel.ERROR);
                    return false;
                }
            }
        }
    }
}
