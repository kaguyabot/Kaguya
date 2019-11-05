using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context;
using System.Data;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries
{
    public static class TestQueries
    {
        public static bool TestConnection()
        {
            using (var db = new KaguyaDb())
            {
                return db.Connection.State.Equals(ConnectionState.Open);
            }
        }
    }
}
