using Discord.WebSocket;
using Humanizer;
using Kaguya.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Kaguya.Internal.Services
{
	public class GreetingService
	{
		private readonly ILogger<GreetingService> _logger;
		private readonly IServiceProvider _serviceProvider;

		public GreetingService(ILogger<GreetingService> logger, IServiceProvider serviceProvider)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
		}

		public async Task SendGreetingAsync(SocketGuildUser user)
		{
			// Apparently, users can be null.
			if (user == null || user.IsBot || user.IsWebhook)
			{
				return;
			}

			using (var scope = _serviceProvider.CreateScope())
			{
				var kaguyaServerRepository = scope.ServiceProvider.GetRequiredService<KaguyaServerRepository>();

				var server = await kaguyaServerRepository.GetOrCreateAsync(user.Guild.Id);

				if (String.IsNullOrWhiteSpace(server.CustomGreeting))
				{
					return;
				}

				var channel = user.Guild.GetTextChannel(server.CustomGreetingTextChannelId.GetValueOrDefault());

				if (channel == null && server.CustomGreetingTextChannelId != null)
				{
					_logger.LogWarning($"Failed to send greeting in guild {user.Guild.Id}. Text " +
					                   "channel was null. Disabling to suppress warnings.");

					server.CustomGreetingTextChannelId = null;
					await kaguyaServerRepository.UpdateAsync(server);

					return;
				}

				string parsedMessage = ParseGreetingString(server.CustomGreeting, user);

				try
				{
					await channel.SendMessageAsync(parsedMessage);
				}
				catch (Exception e)
				{
					_logger.LogDebug(e,
						$"Exception encountered when sending greeting message to {channel.Id} " + $"in guild {server.ServerId}.");
				}
			}
		}

		/// <summary>
		///  Parses and returns the greeting string. Replaces variables as follows:
		///  - {USERMENTION} -> Mentions the user\n" +
		///  - {MEMBERCOUNT} -> The count of members in the server, formatted as 1st, 2nd, 3rd, 4th\n" +
		///  - {SERVERNAME} -> The name of the server"
		/// </summary>
		/// <param name="message"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		private string ParseGreetingString(string message, SocketGuildUser user)
		{
			return message.Replace("{USERMENTION}", user.Mention)
			              .Replace("{MEMBERCOUNT}", user.Guild.MemberCount.Ordinalize())
			              .Replace("{SERVERNAME}", user.Guild.Name);
		}
	}
}