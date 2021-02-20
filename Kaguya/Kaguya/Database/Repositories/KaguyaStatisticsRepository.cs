using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Repositories
{
    public class KaguyaStatisticsRepository : RepositoryBase<KaguyaStatistics>, IKaguyaStatisticsRepository
    {
        private readonly KaguyaUserRepository _kaguyaUserRepository;
        private readonly KaguyaServerRepository _kaguyaServerRepository;
        private readonly DiscordShardedClient _client;
        private readonly CommandHistoryRepository _commandHistoryRepository;
        private readonly FishRepository _fishRepository;
        private readonly GambleHistoryRepository _gambleHistoryRepository;

        public KaguyaStatisticsRepository(KaguyaDbContext dbContext, KaguyaUserRepository kaguyaUserRepository,
            KaguyaServerRepository kaguyaServerRepository, DiscordShardedClient client,
            CommandHistoryRepository commandHistoryRepository, FishRepository fishRepository,
            GambleHistoryRepository gambleHistoryRepository) : base(dbContext)
        {
            _kaguyaUserRepository = kaguyaUserRepository;
            _kaguyaServerRepository = kaguyaServerRepository;
            _client = client;
            _commandHistoryRepository = commandHistoryRepository;
            _fishRepository = fishRepository;
            _gambleHistoryRepository = gambleHistoryRepository;
        }

        public async Task PostNewAsync()
        {
            Process proc = Process.GetCurrentProcess();
            double ramUsage = (double)proc.PrivateMemorySize64 / 1000000; // Megabyte.
            
            int users = await _kaguyaUserRepository.GetCountAsync();
            int servers = await _kaguyaServerRepository.GetCountAsync();
            int curServers = _client.Guilds.Count;
            int shards = _client.Shards.Count;
            int commandsExecuted = await _commandHistoryRepository.GetSuccessfulCountAsync();
            int fish = await _fishRepository.GetCountAsync();
            int gambles = await _gambleHistoryRepository.GetCountAsync();
            int latency = _client.Latency;
            long coins = await _kaguyaUserRepository.CountCoinsAsync();

            var newStats = new KaguyaStatistics
            {
                Users = users,
                Servers = servers,
                ConnectedServers = curServers,
                Shards = shards,
                CommandsExecuted = commandsExecuted,
                Fish = fish,
                Coins = coins,
                Gambles = gambles,
                RamUsageMegabytes = ramUsage,
                LatencyMilliseconds = latency,
                Version = Global.Version,
                Timestamp = DateTimeOffset.Now
            };

            await InsertAsync(newStats);
        }

        public async Task<KaguyaStatistics> GetMostRecentAsync()
        {
            return await Table.AsQueryable().OrderByDescending(x => x.Id).FirstOrDefaultAsync();
        }
    }
}