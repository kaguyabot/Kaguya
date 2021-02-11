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
    [Module(CommandModule.Exp)]
    [Group("daily")]
    [Alias("d")]
    public class Daily : KaguyaBase<Daily>
    {
        private readonly ILogger<Daily> _logger;
        private readonly KaguyaUserRepository _kaguyaUserRepository;

        private const int POINTS_GIVEN = 1250;
        private const int EXP_GIVEN = 250;
        
        public Daily(ILogger<Daily> logger, KaguyaUserRepository kaguyaUserRepository) : base(logger)
        {
            _logger = logger;
            _kaguyaUserRepository = kaguyaUserRepository;
        }

        [Command]
        [Summary("Gives you bonus points and global EXP. Can be used once per day.")]
        public async Task DailyCommandAsync()
        {
            var user = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);

            if (!user.CanGetDailyPoints)
            {
                string diff = (user.LastDailyBonus!.Value - DateTime.Now.AddHours(-24)).HumanizeTraditionalReadable();
                await SendBasicErrorEmbedAsync($"Sorry, you must wait {diff.AsBold()} to use this command again.");
                return;
            }
            
            user.AdjustPoints(POINTS_GIVEN);
            user.AdjustExperienceGlobal(EXP_GIVEN);
            user.LastDailyBonus = DateTime.Now;

            await _kaguyaUserRepository.UpdateAsync(user);
            _logger.LogDebug($"User {user.UserId} has received {POINTS_GIVEN} points and {EXP_GIVEN} exp for their daily bonus. " +
                             $"They may redeem again at {DateTime.Now.AddHours(24)}.");

            var embed = GetBasicSuccessEmbedBuilder("You have claimed your daily bonus!\nYou may claim again in 24 hours.\n" +
                                                    $"Bonus: {POINTS_GIVEN.ToString("N0").AsBold()} points and {EXP_GIVEN.ToString("N0").AsBold()} exp.")
                .WithFooter(new EmbedFooterBuilder
                {
                    Text = $"New total points: {user.Points:N0} | New total exp: {user.GlobalExp:N0}"
                });

            await SendEmbedAsync(embed);
        }
    }
}