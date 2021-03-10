using Discord;
using Discord.Commands;
using Kaguya.Database.Repositories;
using Kaguya.External.Services.TopGg;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Reference
{
	[Module(CommandModule.Reference)]
	[Group("vote")]
	[Alias("upvote", "uv")]
	[Summary("Displays a link to vote for Kaguya on top.gg. Voting online automatically " + "earns you rewards!")]
	public class Vote : KaguyaBase<Vote>
	{
		private readonly KaguyaUserRepository _kaguyaUserRepository;

		public Vote(ILogger<Vote> logger, KaguyaUserRepository kaguyaUserRepository) : base(logger)
		{
			_kaguyaUserRepository = kaguyaUserRepository;
		}

		[Command]
		[InheritMetadata(CommandMetadata.Summary)]
		public async Task UpvoteLinkCommandAsync()
		{
			const string url = Global.TopGgUpvoteUrl;
			var user = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);

			var topGgSb = new StringBuilder("top.gg".AsBoldUnderlined() + "\n");

			if (user.CanUpvote)
			{
				topGgSb.AppendLine("Available Now!".AsBlueCode(url));
			}
			else
			{
				// They must have an upvote cooldown value if eligible to vote.
				topGgSb.AppendLine(
					$"Cooldown: {user.Cooldowns.TopGgVoteCooldown!.Value.HumanizeTraditionalReadable()}");
			}

			const int coins = UpvoteNotifierService.Coins;
			const int exp = UpvoteNotifierService.Exp;

			var embed = new KaguyaEmbedBuilder(KaguyaColors.LightYellow)
			{
				Title = "Vote for Kaguya",
				Description = topGgSb.ToString(),
				Fields = new List<EmbedFieldBuilder>
				{
					new()
					{
						Name = "Rewards",
						Value = $"`{coins} coins`\n" + $"`{exp} global exp`"
					}
				}
			}.WithFooter("Earn 2x rewards on weekends!");

			await SendEmbedAsync(embed);
		}
	}
}