using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Application;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace KaguyaProjectV2.KaguyaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KaguyaWebContentController : ControllerBase
    {
        // GET: api/<controller>
        [HttpGet]
        public async Task<string> Get()
        {
            KaguyaStatistics data = MemoryCache.MostRecentStats;

            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }
    }
}