using System;
using System.Collections.Generic;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Parsers;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;

namespace Kaguya.Discord.Commands.Reference
{
    [Module(CommandModule.Reference)]
    [Group("remindme")]
    [Alias("remind")]
    public class RemindMe : KaguyaBase<RemindMe>
    {
        private readonly ILogger<RemindMe> _logger;
        private readonly ReminderRepository _reminderRepository;
        
        public RemindMe(ILogger<RemindMe> logger, ReminderRepository reminderRepository) : base(logger)
        {
            _logger = logger;
            _reminderRepository = reminderRepository;
        }

        [Command]
        [Summary("Sets a reminder to be sent to your DM after the specified time. Please allow messages " +
                 "from server members for this feature to work correctly.")]
        [Remarks("<delay time> <text>")]
        public async Task RemindCommand(string duration, [Remainder]string text)
        {
            TimeParser timeParser = new TimeParser(duration);
            TimeSpan parsedTime = timeParser.ParseTime();
            
            Reminder reminder = new Reminder
            {
                UserId = Context.User.Id,
                Text = text,
                Expiration = DateTime.Now.Add(parsedTime),
                HasTriggered = false
            };

            string formattedTime = timeParser.FormattedTimestring();

            await _reminderRepository.InsertAsync(reminder);

            var embed = new KaguyaEmbedBuilder(KaguyaColors.Green)
            {
                Title = "Reminder Set",
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Duration",
                        Value = formattedTime
                    },
                    new EmbedFieldBuilder
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