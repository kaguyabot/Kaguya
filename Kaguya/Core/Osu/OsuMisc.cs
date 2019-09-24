using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Core.Osu
{
    public static class OsuMisc
    {
        public enum AllMods
        {
            NM = 0,
            NF = (1 << 0),
            EZ = (1 << 1),
            //TouchDevice = (1 << 2),
            HD = (1 << 3),
            HR = (1 << 4),
            SD = (1 << 5),
            DT = (1 << 6),
            //Relax = (1 << 7),
            HT = (1 << 8),
            NC = (1 << 9), // Only set along with DoubleTime. i.e: NC only gives 576
            FL = (1 << 10),
            // Autoplay = (1 << 11),
            SO = (1 << 12),
            // Relax2 = (1 << 13),  // Autopilot
            PF = (1 << 14),
        }

        public static string OsuGrade(string grade)
        {
            switch (grade)
            {
                case "XH":
                    return "<:XH:553119188089176074>";
                case "X":
                    return "<:X_:553119217109565470>";
                case "SH":
                    return "<:SH:553119233463025691>";
                case "S":
                    return "<:S_:553119252329267240>";
                case "A":
                    return "<:A_:553119274256826406>";
                case "B":
                    return "<:B_:553119304925577228>";
                case "C":
                    return "<:C_:553119325565878272>";
                case "D":
                    return "<:D_:553119338035675138>";
            }
            return "";
        }

        public static string OsuAccuracy(int count50, int count100, int count300, int countMiss)
        {
            return (100 * ((50 * count50) + (100 * count100) + (300 * count300)) / ((300 * (countMiss + count50 + count100 + count300)))).ToString("F");
        }
    }
}
