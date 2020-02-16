using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaApi.Database.Context;
using KaguyaProjectV2.KaguyaApi.Database.Models;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Handlers.UpvoteHandler;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KaguyaProjectV2.KaguyaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TopGgController : ControllerBase
    {
        private readonly IOptions<KaguyaConfig> _cfg;
        private readonly KaguyaDb _db;
        private readonly UpvoteNotifier _uvNotifier;

        public TopGgController(IOptions<KaguyaConfig> cfg, KaguyaDb db, UpvoteNotifier uvNotifier)
        {
            _cfg = cfg;
            _db = db;
            _uvNotifier = uvNotifier;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost("webhook")]
        public async Task Post([FromBody]TopGgWebhook baseHook, [FromHeader(Name = "Authorization")]string auth)
        {
            if (auth != _cfg.Value.TopGGAuthorization)
                return;

            var dbWebhook = new DatabaseUpvoteWebhook()
            {
                BotId = baseHook.BotId.AsUlong(),
                UserId = baseHook.UserId.AsUlong(),
                UpvoteType = baseHook.Type,
                IsWeekend = baseHook.IsWeekend,
                QueryParams = baseHook.Query,
                TimeVoted = DateTime.Now.ToOADate(),
                VoteId = Guid.NewGuid().ToString()
            };

            await _db.InsertAsync(dbWebhook);
            _uvNotifier.Enqueue(dbWebhook);

            await ConsoleLogger.LogAsync($"[Kaguya Api]: Authorized Top.GG Webhook received for user {dbWebhook.UserId}.", LogLvl.INFO);
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
