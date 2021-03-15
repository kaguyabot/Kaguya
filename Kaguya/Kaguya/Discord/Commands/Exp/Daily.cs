using Discord;
using Discord.Commands;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Exp
{
	[Module(CommandModule.Exp)]
	[Group("daily")]
	[Alias("d")]
	public class Daily : KaguyaBase<Daily>
	{
		private const int COINS_GIVEN = 750;
		private const int EXP_GIVEN = 250;
		private readonly KaguyaUserRepository _kaguyaUserRepository;
		private readonly ILogger<Daily> _logger;

		public Daily(ILogger<Daily> logger, KaguyaUserRepository kaguyaUserRepository) : base(logger)
		{
			_logger = logger;
			_kaguyaUserRepository = kaguyaUserRepository;
		}

		[Command]
		[Summary("Gives you bonus coins and global EXP. Can be used once per day.")]
		public async Task DailyCommandAsync()
		{
			var user = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);

			if (!user.CanGetDailyCoins)
			{
				string diff = (user.LastDailyBonus!.Value - DateTimeOffset.Now.AddHours(-24)).HumanizeTraditionalReadable();

				await SendBasicErrorEmbedAsync($"Sorry, you must wait {diff.AsBold()} to use this command again.");
				return;
			}

			user.AdjustCoins(COINS_GIVEN);
			user.AdjustExperienceGlobal(EXP_GIVEN);
			user.LastDailyBonus = DateTimeOffset.Now;

			await _kaguyaUserRepository.UpdateAsync(user);
			_logger.LogDebug($"User {user.UserId} has received {COINS_GIVEN} coins and {EXP_GIVEN} exp for their daily bonus. " +
			                 $"They may redeem again at {DateTimeOffset.Now.AddHours(24)}.");

			var embed = GetBasicSuccessEmbedBuilder("You have claimed your daily bonus!\nYou may claim again in 24 hours.\n" +
			                                        $"Bonus: {COINS_GIVEN.ToString("N0").AsBold()} coins and {EXP_GIVEN.ToString("N0").AsBold()} exp.")
				.WithFooter(new EmbedFooterBuilder
				{
					Text = $"New total coins: {user.Coins:N0} | New total exp: {user.GlobalExp:N0}"
				});

			await SendEmbedAsync(embed);
		}
	}
}