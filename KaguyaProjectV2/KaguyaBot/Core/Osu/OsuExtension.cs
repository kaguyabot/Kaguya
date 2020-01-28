using System;
using Humanizer;
using Humanizer.Localisation;

namespace KaguyaProjectV2.KaguyaBot.Core.Osu
{
    public enum OsuRequest
    {
        USER,
        BEST_PERFORMANCE,
        RECENT_PLAYED,
    }

    public static class OsuExtension
    {
        public static string ModeNames(int modnumber)
        {
            string modString = modnumber > 0 ? "+" : "";

            if (isBitSet(modnumber, 0))
                modString += "NF";
            if (isBitSet(modnumber, 1))
                modString += "EZ";
            if (isBitSet(modnumber, 8))
                modString += "HT";
            if (isBitSet(modnumber, 3))
                modString += "HD";
            if (isBitSet(modnumber, 4))
                modString += "HR";
            if (isBitSet(modnumber, 6) && !isBitSet(modnumber, 9))
                modString += "DT";
            if (isBitSet(modnumber, 9))
                modString += "NC";
            if (isBitSet(modnumber, 10))
                modString += "FL";
            if (isBitSet(modnumber, 5))
                modString += "SD";
            if (isBitSet(modnumber, 14))
                modString += "PF";
            if (isBitSet(modnumber, 7))
                modString += "RX";
            if (isBitSet(modnumber, 11))
                modString += "AT";
            if (isBitSet(modnumber, 12))
                modString += "SO";
            return modString;

            static bool isBitSet(int mods, int pos) =>
                (mods & (1 << pos)) != 0;
        }

        public static string OsuGrade(string grade)
        {
            return grade switch
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

        public static double OsuAccuracy(int count50, int count100, int count300, int countMiss)
        {
            return (double)100 * ((50 * count50) + (100 * count100) + (300 * count300)) / (300 * (countMiss + count50 + count100 + count300));
        }

        public static string ToTimeAgo(TimeSpan time)
        {
            return time.Humanize(minUnit: TimeUnit.Second, maxUnit: TimeUnit.Year, precision: 7);
        }
    }
}
