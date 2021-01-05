using System;
using System.Collections.Generic;
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
        
        protected string GetModString(Mode input)
        {
            var newMods = new List<string>();
            Array enumCollection = Enum.GetValues(input.GetType());
            
            foreach (Enum value in enumCollection)
            {
                if (input.HasFlag(value))
                {
                    Mode val = value is Mode mod ? mod : Mode.None;

                    if (val == Mode.None && enumCollection.Length == 1)
                    {
                        return "No Mod";
                    }

                    if (val == Mode.None && enumCollection.Length > 1)
                    {
                        continue;
                    }
                    
                    newMods.Add(val.ToModeString(_osuClient));
                }
            }

            // Double checking, no-mod may have been skipped.
            if (newMods.Count == 0)
            {
                return "No Mod";
            }

            string final = string.Empty;
            foreach (string modString in newMods)
            {
                final += $"{modString}, ";
            }

            return final[..^2];
        }

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

        /// <summary>
        /// Gets the correct <see cref="TimeSpan"/> for when this osu! play was submitted.
        /// </summary>
        /// <returns></returns>
        protected TimeSpan GetScoreTimespan(Score score)
        {
            return score.Date.HasValue ? DateTime.UtcNow - score.Date.Value.DateTime : TimeSpan.Zero;
        }
    }
}