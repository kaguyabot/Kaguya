using Discord.Commands;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Reference
{
	[Module(CommandModule.Reference)]
	[Group("cooldowns")]
	[Alias("cd", "cds")]
	public class Cooldowns : KaguyaBase<Cooldowns>
	{
		private readonly CommonEmotes _commonEmotes;
		private readonly KaguyaUserRepository _kaguyaUserRepository;

		public Cooldowns(ILogger<Cooldowns> logger, KaguyaUserRepository kaguyaUserRepository, CommonEmotes commonEmotes) : base(logger)
		{
			_kaguyaUserRepository = kaguyaUserRepository;
			_commonEmotes = commonEmotes;
		}

		[Command]
		[Summary("Displays all of your current cooldowns for time-restricted events, such as the `daily` command.")]
		public async Task CooldownsCommandAsync()
		{
			var user = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);
			var cooldowns = user.Cooldowns;

			var desc = new StringBuilder();
			foreach (var cooldown in cooldowns.ToList())
			{
				// Has the cooldown expired? If so, let them know. If not, display cooldown time.
				desc.Append($"{cooldown}: ")
				    .AppendLine(cooldown.HasExpired()
					    ? $"{_commonEmotes.CheckMarkEmoji} " + "EXPIRED".AsBlueCode()
					    : $"{_commonEmotes.RedCrossEmote} " + cooldown.CooldownRemaining()!.Value.HumanizeTraditionalReadable());
			}

			var embed = new KaguyaEmbedBuilder().WithDescription($"{Context.User.Mention}'s Cooldowns".AsBoldUnderlined() + "\n\n" + desc);

			await SendEmbedAsync(embed);
		}
	}
}