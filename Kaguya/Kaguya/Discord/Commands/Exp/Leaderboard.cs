using System.Text;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord.WebSocket;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Internal.Enums;

namespace Kaguya.Discord.Commands.Exp
{
    [Module(CommandModule.Exp)]
    [Group("leaderboard")]
    [Alias("lb")]
    public class Leaderboard : KaguyaBase<Leaderboard>
    {
        private readonly ILogger<Leaderboard> _logger;
        private readonly KaguyaUserRepository _kaguyaUserRepository;
        private readonly FishRepository _fishRepository;

        private const int LEADERBOARD_COUNT = 10;

        public Leaderboard(ILogger<Leaderboard> logger, KaguyaUserRepository kaguyaUserRepository,
            FishRepository fishRepository) : base(logger)
        {
            _logger = logger;
            _kaguyaUserRepository = kaguyaUserRepository;
            _fishRepository = fishRepository;
        }

        [Command("-coins")]
        [Summary("Displays the top 10 Kaguya coin holders.")]
        public async Task CoinsLeaderboardCommandAsync()
        {
            var topHolders = await _kaguyaUserRepository.GetTopCoinHoldersAsync(20);
            var descSb = new StringBuilder();

            for (int i = 0; i < topHolders.Count; i++)
            {
                if (i + 1 == LEADERBOARD_COUNT)
                {
                    break;
                }
                
                var kaguyaUser = topHolders[i];
                var socketUser = Context.Client.GetUser(topHolders[i].UserId);

                if (kaguyaUser == null || socketUser == null)
                {
                    continue;
                }
                
                if (i == 0)
                {
                    descSb.AppendLine($"{i + 1}. {socketUser.Username} - {kaguyaUser.Coins:N0} coins".AsBold());
                }
                else
                {
                    descSb.AppendLine($"{i + 1}. {socketUser.Username ?? "Invalid user"} - {kaguyaUser.Coins:N0} coins");
                }
            }
            
            var embed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
            {
                Title = "Kaguya Coins Leaderboard",
                Description = descSb.ToString()
            };

            await SendEmbedAsync(embed);
        }
        
        [Command("-exp")]
        [Summary("Displays the top 10 Kaguya exp holders.")]
        public async Task ExpLeaderboardCommandAsync()
        {
            var topHolders = await _kaguyaUserRepository.GetTopExpHoldersAsync();
            var descSb = new StringBuilder();

            for (int i = 0; i < topHolders.Count; i++)
            {
                if (i + 1 == LEADERBOARD_COUNT)
                {
                    break;
                }
                
                var kaguyaUser = topHolders[i];
                var socketUser = Context.Client.GetUser(topHolders[i].UserId);

                if (kaguyaUser == null || socketUser == null)
                {
                    continue;
                }
                
                if (i == 0)
                {
                    descSb.AppendLine($"{i + 1}. {socketUser.Username} - {kaguyaUser.GlobalExp:N0} exp".AsBold());
                }
                else
                {
                    descSb.AppendLine($"{i + 1}. {socketUser.Username} - {kaguyaUser.GlobalExp:N0} exp");
                }
            }
            
            var embed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
            {
                Title = "Kaguya Exp Leaderboard",
                Description = descSb.ToString()
            };

            await SendEmbedAsync(embed);
        }
        
        [Command("-fish")]
        [Summary("Displays the top 10 Kaguya fish holders.")]
        public async Task FishLeaderboardCommandAsync()
        {
            var topHolders = await _kaguyaUserRepository.GetTopFishHoldersAsync();
            var descSb = new StringBuilder();

            for (int i = 0; i < topHolders.Count; i++)
            {
                if (i + 1 == LEADERBOARD_COUNT)
                {
                    break;
                }
                
                var kaguyaUser = topHolders[i];
                var socketUser = Context.Client.GetUser(topHolders[i].UserId);

                if (kaguyaUser == null || socketUser == null)
                {
                    continue;
                }
                
                int userFish = await _fishRepository.CountAllNonTrashAsync(kaguyaUser.UserId);
                
                if (i == 0)
                {
                    descSb.AppendLine($"{i + 1}. {socketUser.Username} - Level {kaguyaUser.FishLevel:N0} - {userFish:N0}x fish".AsBold());
                }
                else
                {
                    descSb.AppendLine($"{i + 1}. {socketUser.Username} - Level {kaguyaUser.FishLevel:N0} - {userFish:N0}x fish");
                }
            }
            
            var embed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
            {
                Title = "Kaguya Fishing Leaderboard",
                Description = "🎣 " + descSb
            };

            await SendEmbedAsync(embed);
        }
    }
}