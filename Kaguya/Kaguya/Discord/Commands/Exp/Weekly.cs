using System;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Kaguya.Database.Repositories;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Internal.Enums;

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

        private const int POINTS_GIVEN = 12000;
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

            if (!user.CanGetWeeklyPoints)
            {
                string diff = (user.LastDailyBonus!.Value - DateTime.Now.AddDays(-7)).HumanizeTraditionalReadable();
                await SendBasicErrorEmbedAsync($"Sorry, you must wait {diff.AsBold()} to use this command again.");
                return;
            }
            
            user.AdjustPoints(POINTS_GIVEN);
            user.AdjustExperienceGlobal(EXP_GIVEN);
            user.LastWeeklyBonus = DateTime.Now;

            await _kaguyaUserRepository.UpdateAsync(user);
            _logger.LogDebug($"User {user.UserId} has received {POINTS_GIVEN} points and {EXP_GIVEN} exp for their weekly bonus. " +
                             $"They may redeem again at {DateTime.Now.AddDays(7)}.");

            var embed = GetBasicSuccessEmbedBuilder("You have claimed your weekly premium bonus!\nYou may claim again in 7 days.\n" +
                                                    $"Bonus: {POINTS_GIVEN.ToString("N0").AsBold()} points and {EXP_GIVEN.ToString("N0").AsBold()} exp.")
                        .WithColor(KaguyaColors.Gold)
                        .WithFooter($"New total points: {user.Points:N0} | New total exp: {user.GlobalExp:N0}");

            await SendEmbedAsync(embed);
        }
    }
}