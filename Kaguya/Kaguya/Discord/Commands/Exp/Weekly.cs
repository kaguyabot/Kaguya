using System;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;

namespace Kaguya.Discord.Commands.Exp
{
    [Restriction(ModuleRestriction.PremiumUser)]
    [Module(CommandModule.Exp)]
    [Group("weekly")]
    [Summary("Allows redemption of a major weekly bonus reward!")]
    public class Weekly : KaguyaBase<Weekly>
    {
        private readonly ILogger<Weekly> _logger;
        private readonly KaguyaUserRepository _kaguyaUserRepository;

        private const int COINS_GIVEN = 5000;
        private const int EXP_GIVEN = 250;
        
        public Weekly(ILogger<Weekly> logger, KaguyaUserRepository kaguyaUserRepository) : base(logger)
        {
            _logger = logger;
            _kaguyaUserRepository = kaguyaUserRepository;
        }

        [Command]
        [InheritMetadata(CommandMetadata.Summary)]
        public async Task WeeklyCommandAsync()
        {
            var user = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);

            if (!user.CanGetWeeklyCoins)
            {
                string diff = (user.LastDailyBonus!.Value - DateTimeOffset.Now.AddDays(-7)).HumanizeTraditionalReadable();
                await SendBasicErrorEmbedAsync($"Sorry, you must wait {diff.AsBold()} to use this command again.");
                return;
            }
            
            user.AdjustCoins(COINS_GIVEN);
            user.AdjustExperienceGlobal(EXP_GIVEN);
            user.LastWeeklyBonus = DateTimeOffset.Now;

            await _kaguyaUserRepository.UpdateAsync(user);
            _logger.LogDebug($"User {user.UserId} has received {COINS_GIVEN} coins and {EXP_GIVEN} exp for their weekly bonus. " +
                             $"They may redeem again at {DateTimeOffset.Now.AddDays(7)}.");

            var embed = GetBasicSuccessEmbedBuilder("You have claimed your weekly premium bonus!\nYou may claim again in 7 days.\n" +
                                                    $"Bonus: {COINS_GIVEN.ToString("N0").AsBold()} coins and {EXP_GIVEN.ToString("N0").AsBold()} exp.")
                        .WithColor(KaguyaColors.Gold)
                        .WithFooter($"New total coins: {user.Coins:N0} | New total exp: {user.GlobalExp:N0}");

            await SendEmbedAsync(embed);
        }
    }
}