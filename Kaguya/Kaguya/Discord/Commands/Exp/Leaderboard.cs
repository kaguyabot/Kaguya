using System.Text;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
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
        
        public Leaderboard(ILogger<Leaderboard> logger, KaguyaUserRepository kaguyaUserRepository) : base(logger)
        {
            _logger = logger;
            _kaguyaUserRepository = kaguyaUserRepository;
        }

        [Command("-coins")]
        [Alias("-p")]
        [Summary("Displays the top 10 Kaguya Coin holders.")]
        public async Task CoinsLeaderboardCommandAsync()
        {
            var topHolders = await _kaguyaUserRepository.GetTopCoinHoldersAsync();
            var descSb = new StringBuilder();

            for (int i = 0; i < topHolders.Count; i++)
            {
                var kaguyaUser = topHolders[i];
                var socketUser = Context.Client.GetUser(topHolders[i].UserId);
                
                if (i == 0)
                {
                    descSb.AppendLine($"{i + 1}. {socketUser.Username} - {kaguyaUser.Coins:N0} coins".AsBold());
                }
                else
                {
                    descSb.AppendLine($"{i + 1}. {socketUser.Username} - {kaguyaUser.Coins:N0} coins");
                }
            }
            
            var embed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
            {
                Title = "Kaguya Coins Leaderboard",
                Description = descSb.ToString()
            };

            await SendEmbedAsync(embed);
        }
    }
}