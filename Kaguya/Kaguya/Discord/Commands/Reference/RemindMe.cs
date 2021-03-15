using Discord;
using Discord.Commands;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Parsers;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Reference
{
	[Module(CommandModule.Reference)]
	[Group("remindme")]
	[Alias("remind")]
	public class RemindMe : KaguyaBase<RemindMe>
	{
		private readonly ReminderRepository _reminderRepository;

		public RemindMe(ILogger<RemindMe> logger, ReminderRepository reminderRepository) : base(logger)
		{
			_reminderRepository = reminderRepository;
		}

		[Command]
		[Summary("Sets a reminder to be sent to your DM after the specified time. Please allow messages " +
		         "from server members for this feature to work correctly.")]
		[Remarks("<delay time> <text>")]
		[Example("2h walk the dog")]
		[Example("14d5h30m25s The game starts now!")]
		public async Task RemindCommand(string duration, [Remainder]
			string text)
		{
			var timeParser = new TimeParser(duration);
			var parsedTime = timeParser.ParseTime();

			var reminder = new Reminder
			{
				UserId = Context.User.Id,
				Text = text,
				Expiration = DateTimeOffset.Now.Add(parsedTime),
				HasTriggered = false
			};

			string formattedTime = timeParser.FormattedTimestring();

			await _reminderRepository.InsertAsync(reminder);

			var embed = new KaguyaEmbedBuilder(KaguyaColors.Green)
			{
				Title = "Reminder Set",
				Fields = new List<EmbedFieldBuilder>
				{
					new()
					{
						Name = "Duration",
						Value = formattedTime
					},
					new()
					{
						Name = "Reminder",
						Value = text
					}
				}
			};

			await SendEmbedAsync(embed);
		}
	}
}