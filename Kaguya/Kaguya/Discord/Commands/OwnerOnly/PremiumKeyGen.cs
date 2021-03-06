using Discord;
using Discord.Commands;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Parsers;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.OwnerOnly
{
	[Restriction(ModuleRestriction.OwnerOnly)]
	[Module(CommandModule.OwnerOnly)]
	[Group("premiumkeygen")]
	[Alias("pgen")]
	public class PremiumKeyGen : KaguyaBase<PremiumKeyGen>
	{
		private readonly PremiumKeyRepository _premiumKeyRepository;

		public PremiumKeyGen(ILogger<PremiumKeyGen> logger, PremiumKeyRepository premiumKeyRepository) : base(logger)
		{
			_premiumKeyRepository = premiumKeyRepository;
		}

		[Command]
		[Summary("Generates the specified amount of premium keys for the specified duration.")]
		[Remarks("<amount> <duration>\n10 30d")]
		public async Task PremiumGenCommand(int amount, string time)
		{
			var parser = new TimeParser(time);

			var parsedTime = parser.ParseTime();
			string timeString = parsedTime.HumanizeTraditionalReadable().AsBold();

			var collection = await _premiumKeyRepository.GenerateAndInsertAsync(Context.User.Id, amount, parsedTime);

			await SendBasicSuccessEmbedAsync($"Successfully bulk-inserted {amount.ToString("N0").AsBold()} {timeString} premium keys.");

			var builder = new StringBuilder();
			foreach (var key in collection)
			{
				builder.AppendLine($"Duration: {key.HumanizedLength.AsBold()} | Key: {key.Key.AsBold()}");
			}

			if (amount < 20)
			{
				var dmEmbed = new KaguyaEmbedBuilder(KaguyaColors.Magenta).WithTitle("Kaguya Premium Keys")
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

						await Context.User.SendFileAsync(ms,
							$"{amount}-{timeString}-PremiumKeys-{DateTimeOffset.Now:yyyy-mm-dd--hh-mm-ss}.txt");
					}
					catch (Exception e)
					{
						await SendBasicErrorEmbedAsync("An error occured while writing key contents to memory to send as a file.\n" +
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