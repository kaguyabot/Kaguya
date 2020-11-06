using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsuSharp;

namespace KaguyaProjectV2.KaguyaBot.Core.Osu
{
    public class OsuBase
    {
        public static OsuClient Client { get; internal set; }
        public static string OsuGradeEmote(string grade) => grade switch
        {
            "XH" => "<:XH:553119188089176074>",
            "X" => "<:X_:553119217109565470>",
            "SH" => "<:SH:553119233463025691>",
            "S" => "<:S_:553119252329267240>",
            "A" => "<:A_:553119274256826406>",
            "B" => "<:B_:553119304925577228>",
            "C" => "<:C_:553119325565878272>",
            "D" => "<:D_:553119338035675138>",
            "F" => "<:F_:557297028263051288>",
            _ => ""
        };
    }
}