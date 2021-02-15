using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Internal.PrimitiveExtensions;
using Kaguya.Internal.Services;
using Microsoft.Extensions.Logging;

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
        private readonly KaguyaStatisticsRepository _kaguyaStatisticsRepository;
        private readonly ServerExperienceRepository _serverExperienceRepository;
        
        private static readonly string[] _lbEmojis = { "🥇", "🥈", "🥉" };

        public Leaderboard(ILogger<Leaderboard> logger, KaguyaUserRepository kaguyaUserRepository,
            FishRepository fishRepository, KaguyaStatisticsRepository kaguyaStatisticsRepository,
            ServerExperienceRepository serverExperienceRepository) : base(logger)
        {
            _logger = logger;
            _kaguyaUserRepository = kaguyaUserRepository;
            _fishRepository = fishRepository;
            _kaguyaStatisticsRepository = kaguyaStatisticsRepository;
            _serverExperienceRepository = serverExperienceRepository;
        }

        [Command("-coins")]
        [Summary("Displays the top 10 Kaguya coin holders.")]
        public async Task CoinsLeaderboardCommandAsync()
        {
            var topHolders = await _kaguyaUserRepository.GetTopCoinHoldersAsync();
            var stats = await _kaguyaStatisticsRepository.GetMostRecentAsync();
            var descSb = new StringBuilder();

            long totalCoins = stats.Coins;
            long topTenSum = 0;
            for (int i = 0; i < topHolders.Count; i++)
            {
                var kaguyaUser = topHolders[i];
                if (kaguyaUser == null)
                {
                    continue;
                }
                
                string userName = Context.Client.GetUser(kaguyaUser.UserId)?.Username ?? $"Unknown ({kaguyaUser.UserId})";
                
                string emote = GetMedallionString(i);
                string rankNum = i < 3 ? emote : emote + $" {i + 1}.";
                string curLine = $"{rankNum} {userName} - {kaguyaUser.Coins.ToShorthandFormat()} coins";
                if (i == 0)
                {
                    curLine = curLine.AsBold();
                }

                topTenSum += kaguyaUser.Coins;

                descSb.AppendLine(curLine);
            }

            double percentOwnedByTop = ((double)topTenSum / totalCoins) * 100;
            
            var embed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
            {
                Description = $"🤑 Wealth Leaderboard".AsBold() + "\n\n" + descSb
            }.WithFooter($"{totalCoins.ToShorthandFormat()} total coins owned | {percentOwnedByTop:N1}% owned by top 10");

            await SendEmbedAsync(embed);
        }
        
        [Command("-exp")]
        [Summary("Displays the top 10 Kaguya exp holders.")]
        public async Task ExpLeaderboardCommandAsync()
        {
            var topHolders = await _kaguyaUserRepository.GetTopExpHoldersAsync();
            var stats = await _kaguyaStatisticsRepository.GetMostRecentAsync();
            var descSb = new StringBuilder();

            for (int i = 0; i < topHolders.Count; i++)
            {
                var kaguyaUser = topHolders[i];
                if (kaguyaUser == null)
                {
                    continue;
                }
                
                string userName = Context.Client.GetUser(kaguyaUser.UserId)?.Username ?? $"Unknown ({kaguyaUser.UserId})";
                
                string emote = GetMedallionString(i);
                string rankNum = i < 3 ? emote : emote + $" {i + 1}.";
                string curLine = $"{rankNum} {userName} - Level {kaguyaUser.GlobalExpLevel:N0} - {kaguyaUser.GlobalExp.ToShorthandFormat()} EXP";
                if (i == 0)
                {
                    curLine = curLine.AsBold();
                }

                descSb.AppendLine(curLine);
            }
            
            var embed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
            {
                Description = $"📢 Top Chatters (Out of {stats.Users.ToShorthandFormat()})".AsBold() + "\n\n" + descSb
            };

            await SendEmbedAsync(embed);
        }
        
        [Command("-serverexp")]
        [Alias("-sxp")]
        [Summary("Displays the top 10 Kaguya exp holders in the current server.")]
        public async Task ServerExpLeaderboardCommandAsync()
        {
            var topHolders = await _serverExperienceRepository.GetTopAsync(Context.Guild.Id);
            var descSb = new StringBuilder();

            int count = await _serverExperienceRepository.GetAllCountAsync(Context.Guild.Id);
            for (int i = 0; i < topHolders.Count; i++)
            {
                var serverExpObj = topHolders[i];
                if (serverExpObj == null)
                {
                    continue;
                }
                
                string userName = Context.Client.GetUser(serverExpObj.UserId)?.Username ?? $"Unknown ({serverExpObj.UserId})";

                int level = (int)ExperienceService.CalculateLevel(serverExpObj.Exp);
                
                string emote = GetMedallionString(i);
                string rankNum = i < 3 ? emote : emote + $" {i + 1}.";
                string curLine = $"{rankNum} {userName} - Level {level:N0} - {serverExpObj.Exp.ToShorthandFormat()} EXP";
                if (i == 0)
                {
                    curLine = curLine.AsBold();
                }

                descSb.AppendLine(curLine);
            }
            
            var embed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
            {
                Description = $"📢 Top Chatters (Out of {count.ToShorthandFormat()}) [{Context.Guild.Name}]".AsBold() + "\n\n" + descSb
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
                var kaguyaUser = topHolders[i];
                if (kaguyaUser == null)
                {
                    continue;
                }
                
                string userName = Context.Client.GetUser(kaguyaUser.UserId)?.Username ?? $"Unknown ({kaguyaUser.UserId})";
                
                int userFish = await _fishRepository.CountAllNonTrashAsync(kaguyaUser.UserId);

                string emote = GetMedallionString(i);
                string rankNum = i < 3 ? emote : emote + $" {i + 1}.";
                string curLine = $"{rankNum} {userName} - Level {kaguyaUser.FishLevel:N0} - {userFish:N0}x fish";
                if (i == 0)
                {
                    curLine = curLine.AsBold();
                }

                descSb.AppendLine(curLine);
            }
            
            var embed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
            {
                Description = "🎣 Fishermen's Ladder".AsBold() + "\n\n" + descSb
            };

            await SendEmbedAsync(embed);
        }

        /// <summary>
        /// Gets the corresponding medal emoji based on the current number
        /// of items in the list.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private static string GetMedallionString(int i)
        {
            string emote = i switch
            {
                0 => _lbEmojis[i],
                1 => _lbEmojis[i],
                2 => _lbEmojis[i],
                var _ => default
            };

            return emote;
        }
    }
}