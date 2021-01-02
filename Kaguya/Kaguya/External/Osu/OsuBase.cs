using System;
using System.Threading.Tasks;
using Kaguya.Discord.DiscordExtensions;
using OsuSharp;

namespace Kaguya.External.Osu
{
    public class OsuBase
    {
        private readonly OsuClient _osuClient;

        protected OsuBase(OsuClient osuClient)
        {
            _osuClient = osuClient;
        }
        
        // todo: Method for mod string

        protected string GetEmoteForRank(string rank)
        {
            return rank switch
            {
                "XH" => "<:rankingXH:794932724703821864>",
                "X" => "<:rankingX:794932724628586516>",
                "SH" => "<:rankingSH:794932724711948298>",
                "S" => "<:rankingS:794932724837646347>",
                "A" => "<:rankingA:794932725554741258>",
                "B" => "<:rankingB:794932725429174272>",
                "C" => "<:rankingC:794932724863467544>",
                "D" => "<:rankingD:794932725089697792>",
                "F" => "<:rankingF:794934802117427241>",
                _ => "<:rankingF:794934802117427241>"
            };
        }
    }

    public class OsuException : Exception
    {
        public OsuException(string msg) : base(msg) { }
    }

    public class OsuUserNotFoundException : Exception
    {
        public OsuUserNotFoundException(string username) : base($"No osu! username or ID match was found for {username.AsBold()}.") { }
    }
}