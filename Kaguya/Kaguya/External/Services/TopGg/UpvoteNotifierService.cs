using Discord.WebSocket;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Kaguya.External.Services.TopGg
{
	public class UpvoteNotifierService
	{
		public const int Coins = 750;
		public const int Exp = 500;
		private static readonly BlockingCollection<Upvote> _voteQueue = new();
		private readonly DiscordShardedClient _client;
		private readonly ILogger<UpvoteNotifierService> _logger;
		private readonly IServiceProvider _serviceProvider;
		private Task _runner;

		public UpvoteNotifierService(ILogger<UpvoteNotifierService> logger, IServiceProvider serviceProvider, DiscordShardedClient client)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
			_client = client;
			_runner = Task.Run(async () => await Run());
		}

		private async Task Run()
		{
			foreach (var vote in _voteQueue.GetConsumingEnumerable())
			{
				try
				{
					using (var scope = _serviceProvider.CreateScope())
					{
						var kaguyaUserRepository = scope.ServiceProvider.GetRequiredService<KaguyaUserRepository>();
						var upvoteRepository = scope.ServiceProvider.GetRequiredService<UpvoteRepository>();

						var user = await kaguyaUserRepository.GetOrCreateAsync(vote.UserId);
						var socketUser = _client.GetUser(user.UserId);

						int coins = Coins;
						int exp = Exp;

						if (vote.IsWeekend)
						{
							coins *= 2;
							exp *= 2;
						}

						if (user.IsPremium)
						{
							coins *= 2;
							exp *= 2;
						}

						user.AdjustCoins(coins);
						user.AdjustExperienceGlobal(exp);
						user.LastUpvotedTopGg = DateTimeOffset.Now;
						user.TotalUpvotesTopGg++;

						if (socketUser != null)
						{
							var dmCh = await socketUser.GetOrCreateDMChannelAsync();

							string weekendStr = "Because you voted on the weekend, you have been given " + "double coins and double exp!";

							var embed = new KaguyaEmbedBuilder(KaguyaColors.IceBlue)
							{
								Title = "Kaguya Upvote Rewards",
								Description = $"Thanks for upvoting me on [top.gg]({Global.TopGgUpvoteUrl})! " +
								              $"You have been awarded `{coins:N0} coins` and `{exp:N0} exp`. " +
								              $"{(vote.IsWeekend ? weekendStr : "")}"
							};

							try
							{
								await dmCh.SendMessageAsync(embed: embed.Build());
							}
							catch (Exception e)
							{
								_logger.LogDebug(e,
									$"Failed to DM user {user.UserId} with their " + "Top.GG authorized vote notification.");
							}
						}

						try
						{
							// todo: Move debug logs into the repositories themselves.
							await kaguyaUserRepository.UpdateAsync(user);
							_logger.LogDebug($"User {user.UserId} has successfully upvoted and been rewarded on top.gg. " +
							                 "Object updated in database.");

							await upvoteRepository.InsertAsync(vote);
							_logger.LogDebug($"Upvote object with ID {vote.Id} has successfully been inserted into the database.");
						}
						catch (Exception e)
						{
							_logger.LogError(e, "Failed to insert authorized Top.GG webhook into database " + $"for user {user.UserId}.");
						}
					}
				}
				catch (Exception e)
				{
					_logger.LogCritical(e, "An exception occurred inside of the top.gg notifier for-each loop");
				}
			}
		}

		public void Enqueue(Upvote item) { _voteQueue.Add(item); }
	}
}