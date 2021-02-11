using System;
using System.Threading.Tasks;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Web.Contracts;
using Kaguya.Web.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kaguya.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TopGgController : ControllerBase
    {
        private readonly IOptions<TopGgConfigurations> _configs;
        private readonly ILogger<TopGgController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public TopGgController(IOptions<TopGgConfigurations> configs, ILogger<TopGgController> logger,
            IServiceProvider serviceProvider)
        {
            _configs = configs;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        
        [HttpPost("webhook")]
        public async Task<IActionResult> Post([FromBody] TopGgWebhookPayload payload, [FromHeader(Name = "Authorization")] string auth)
        {
            if (string.IsNullOrWhiteSpace(_configs.Value.ApiKey) || string.IsNullOrWhiteSpace(_configs.Value.AuthHeader) ||
                auth != _configs.Value.AuthHeader)
            {
                return Unauthorized();
            }

            _logger.LogInformation($"Authorized Top.GG Webhook received for user {payload.UserId}.");

            try
            { 
                var vote = new Upvote
                {
                    UserId = ulong.Parse(payload.UserId),
                    BotId = ulong.Parse(payload.BotId),
                    IsWeekend = payload.IsWeekend,
                    Timestamp = DateTime.Now,
                    QueryParams = payload.Query,
                    ReminderSent = false,
                    Type = payload.Type
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

            return Ok();
        }
    }
}