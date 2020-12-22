using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Humanizer.Localisation;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Attributes;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Discord.Parsers;
using Microsoft.Extensions.Logging;

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
                    LengthInSeconds = (int)parsedTime.TotalSeconds,
                    Expiration = DateTime.Now.Add(parsedTime),
                    UserId = 0,
                    ServerId = 0
                });
            }
            
            await _premiumKeyRepository.BulkInsert(collection);

            string timeString = parsedTime.Humanize(3, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day).AsBold();

            await SendBasicSuccessEmbedAsync($"Successfully bulk-inserted {amount.ToString("N0").AsBold()} {timeString} premium keys.");

            var builder = new StringBuilder();
            foreach (PremiumKey key in collection)
            {
                builder.AppendLine($"Duration: {key.HumanizedDuration.AsBold()} | Key: {key.Key.AsBold()}");
            }
            
            if (amount < 20)
            {
                var dmEmbed = new KaguyaEmbedBuilder(Color.Magenta)
                              .WithTitle("Kaguya Premium Keys")
                              .WithDescription(builder.ToString())
                              .WithCurrentTimestamp()
                              .Build();

                await Context.User.SendEmbedAsync(dmEmbed);
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    var sw = new StreamWriter(ms);
                    try
                    {
                        builder = builder.Replace("*", "");
                        sw.Write(builder.ToString());
                        sw.Flush();
                        ms.Seek(0, SeekOrigin.Begin);
                        
                        await Context.User.SendFileAsync(ms, $"{amount}-{timeString}-PremiumKeys-{DateTime.Now:yyyy-mm-dd--hh-mm-ss}.txt");
                    }
                    catch (Exception e)
                    {
                        await SendBasicErrorEmbedAsync($"An error occured while writing key contents to memory to send as a file.\n" +
                                                       $"Error: {e.ToString().AsBold()}");
                    }
                    finally
                    {
                        sw.Dispose();
                    }
                }
            }
        }
    }
}