using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaApi.Database.Models;
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
            var data = new KaguyaWebData();
            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }
    }
}