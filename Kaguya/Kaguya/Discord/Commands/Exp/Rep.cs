using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
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
	[Group("rep")]
	public class Rep : KaguyaBase<Rep>
	{
		private readonly KaguyaUserRepository _kaguyaUserRepository;
		private readonly RepRepository _repRepository;

		public Rep(ILogger<Rep> logger, KaguyaUserRepository kaguyaUserRepository, RepRepository repRepository) :
			base(logger)
		{
			_kaguyaUserRepository = kaguyaUserRepository;
			_repRepository = repRepository;
		}

		[Command]
		[Summary("Allows you to give rep to another user. Limit 1 per 24 hours. The user must be in the " +
		         "Discord server this command is used from. An optional reason may be " +
		         "passed through at the end. Use without the user parameter to view your own rep.")]
		[Remarks("[user] [reason]")]
		public async Task RepCommand(SocketGuildUser user,
			[Remainder]
			string reason = "No reason provided.")
		{
			if (user.IsEqual(Context.User))
			{
				await SendBasicErrorEmbedAsync("You cannot give rep to yourself.");

				return;
			}

			var curUser = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);
			var nextUser = await _kaguyaUserRepository.GetOrCreateAsync(user.Id);

			if (!curUser.CanGiveRep)
			{
				var difference = curUser.LastGivenRep - DateTimeOffset.Now.AddHours(-24);
				await SendBasicErrorEmbedAsync("Sorry, you need to wait " +
				                               difference.Value.Humanize(2).AsBold() +
				                               " before giving rep again.");
			}
			else
			{
				var rep = new Database.Model.Rep
				{
					UserId = nextUser.UserId,
					GivenBy = curUser.UserId,
					TimeGiven = DateTimeOffset.Now,
					Reason = reason
				};

				curUser.LastGivenRep = DateTimeOffset.Now;

				await _kaguyaUserRepository.UpdateAsync(curUser);
				await _kaguyaUserRepository.UpdateAsync(nextUser);
				await _repRepository.InsertAsync(rep);

				await SendBasicSuccessEmbedAsync($"Gave +1 rep to {user.Mention}!");
			}
		}

		[Command]
		public async Task RepCommand()
		{
			var curUser = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);
			int repNum = await _repRepository.GetCountRepReceivedAsync(curUser.UserId);

			var recentMatch = await _repRepository.GetMostRecentAsync(curUser.UserId);
			bool showFooter = recentMatch != null;

			var embed = new KaguyaEmbedBuilder(KaguyaColors.Green)
			{
				Description = $"{Context.User.Mention} you have " + repNum.ToString().AsBold() + " rep."
			};

			if (showFooter)
			{
				var guildRecentMatch = Context.Guild.GetUser(recentMatch.GivenBy);

				string byText = guildRecentMatch != null ? guildRecentMatch.ToString() : recentMatch.UserId.ToString();

				var lastRepped = DateTimeOffset.Now - recentMatch.TimeGiven;
				embed.Footer = new EmbedFooterBuilder
				{
					Text = $"Last given rep {lastRepped.Humanize()} ago by {byText}"
				};
			}

			await SendEmbedAsync(embed);
		}
	}
}