using System;
using System.Collections.Generic;
using System.IO;
using Discord.Commands;
using Kaguya.Discord.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Localisation;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Discord.Parsers;

namespace Kaguya.Discord.Commands.OwnerOnly
{
    [Module(CommandModule.OwnerOnly)]
    [Group("premiumkeygen")]
    [Alias("pgen")]
    public class PremiumKeyGen : KaguyaBase<PremiumKeyGen>
    {
        private readonly ILogger<PremiumKeyGen> _logger;
        private readonly PremiumKeyRepository _premiumKeyRepository;
        
        public PremiumKeyGen(ILogger<PremiumKeyGen> logger, PremiumKeyRepository premiumKeyRepository) : base(logger)
        {
            _logger = logger;
            _premiumKeyRepository = premiumKeyRepository;
        }

        [Command]
        [Summary("Generates the specified amount of premium keys for the specified duration.")]
        [Remarks("<amount> <duration>\n10 30d")]
        public async Task PremiumGenCommand(int amount, string time)
        {
            var collection = new List<PremiumKey>();
            var parser = new TimeParser(time);
            TimeSpan parsedTime = parser.ParseTime();
            
            for (int i = 0; i < amount; i++)
            {
                collection.Add(new PremiumKey
                {
                    Key = PremiumKey.GenerateKey(),
                    KeyCreatorId = Context.User.Id,
                    LengthInSeconds = (long)parsedTime.TotalSeconds,
                    Expiration = DateTime.Now.Add(parsedTime),
                    UserId = 0,
                    ServerId = 0
                });
            }
            
            await _premiumKeyRepository.BulkInsert(collection);

            string timeString = parsedTime.Humanize(3, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day).AsBold();

            await SendBasicSuccessEmbedAsync($"Successfully bulk-inserted {amount.ToString("N0").AsBold()} {timeString} premium keys.");

            if (amount < 20)
            {
                //var file = new FileStream($"");
            }
        }
    }
}