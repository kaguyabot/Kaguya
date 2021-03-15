using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.External.Services.TopGg;
using Kaguya.Internal.Events;
using Kaguya.Web.Contracts;
using Kaguya.Web.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Kaguya.Web.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class TopGgController : ControllerBase
	{
		private readonly IOptions<TopGgConfigurations> _configs;
		private readonly ILogger<TopGgController> _logger;
		private readonly IServiceProvider _serviceProvider;

		public TopGgController(IOptions<TopGgConfigurations> configs, ILogger<TopGgController> logger, IServiceProvider serviceProvider)
		{
			_configs = configs;
			_logger = logger;
			_serviceProvider = serviceProvider;
		}

		[HttpPost("webhook")]
		public async Task<IActionResult> Post([FromBody]
			TopGgWebhookPayload payload, [FromHeader(Name = "Authorization")]
			string auth)
		{
			if (string.IsNullOrWhiteSpace(_configs.Value.ApiKey) ||
			    string.IsNullOrWhiteSpace(_configs.Value.AuthHeader) ||
			    auth != _configs.Value.AuthHeader)
			{
				_logger.LogInformation("Unauthorized top.gg post request received");
				return Unauthorized();
			}

			_logger.LogInformation($"Authorized Top.GG Webhook received for user {payload.UserId}.");

			Upvote vote = null;
			try
			{
				vote = new Upvote
				{
					UserId = ulong.Parse(payload.UserId),
					BotId = ulong.Parse(payload.BotId),
					IsWeekend = payload.IsWeekend,
					Timestamp = DateTimeOffset.Now,
					QueryParams = payload.Query,
					ReminderSent = false,
					Type = payload.Type,
					CoinsAwarded = UpvoteNotifierService.Coins,
					ExpAwarded = UpvoteNotifierService.Exp
				};

				using (var scope = _serviceProvider.CreateScope())
				{
					var upvoteRepository = scope.ServiceProvider.GetRequiredService<UpvoteRepository>();
					await upvoteRepository.InsertAsync(vote);
				}
			}
			catch (Exception e)
			{
				_logger.LogWarning(e, $"Failed to parse and upload top.gg webhook for user {payload.UserId}!");
			}

			if (vote != null)
			{
				// Triggers the OnUpvote event, triggering the
				// Kaguya.External.Services.TopGg.UpvoteNotifierService Enqueue() method.
				// This method is responsible for DMing the user and awarding points.
				KaguyaEvents.OnUpvoteTrigger(vote);
			}

			return Ok();
		}
	}
}