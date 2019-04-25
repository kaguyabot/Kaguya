using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using System.Net;
using Kaguya.Core.Commands;

namespace Kaguya.Modules.Patreon
{
    [Group("patreon")]
    public class Patreon : ModuleBase<SocketCommandContext>
    {
        [Command()]
        public async Task PatreonDM()
        {
            var patreonAccessToken = Config.bot.patreonaccesstoken;
            var patreonClientID = Config.bot.patreonclientid;

            WebRequest request = WebRequest.Create($"www.patreon.com/oauth2/authorize?response_type=code&client_id={patreonClientID}" +
                $"&redirect_uri=https://discordapp.com/oauth2/authorize?client_id=538910393918160916&scope=bot&permissions=2146958847");

            WebResponse response = request.GetResponse();

            Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            using (Stream dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                Console.WriteLine(responseFromServer);
            }

            response.Close();
        }
    }
}
