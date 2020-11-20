using System;
using System.Threading.Tasks;
using Kaguya.Web.Contracts;
using Kaguya.Web.Options;
using Microsoft.AspNetCore.Mvc;
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

        public TopGgController(IOptions<TopGgConfigurations> configs, ILogger<TopGgController> logger)
        {
            _configs = configs;
            _logger = logger;
        }
        
        [HttpPost("webhook")]
        public async Task<IActionResult> Post([FromBody] TopGgWebhookPayload baseHook, [FromHeader(Name = "Authorization")] string auth)
        {
            if (string.IsNullOrWhiteSpace(_configs.Value.ApiKey) || auth != _configs.Value.ApiKey)
            {
                return Unauthorized();
            }

            _logger.LogInformation($"Authorized Top.GG Webhook received for user NOT_IMPLEMENTED.");
            
            // TODO: implement
            await Task.Delay(0);
            throw new NotImplementedException();
        }
    }
}