using OsuSharp;

namespace KaguyaProjectV2.KaguyaBot.Core.Osu
{
    public class OsuBase
    {
        public static OsuClient Client { get; internal set; }

        public static string ModeNames(int modnumber)
        {
            string modString = modnumber > 0 ? "+" : "";

            if (IsBitSet(modnumber, 0))
                modString += "NF";

            if (IsBitSet(modnumber, 1))
                modString += "EZ";

            if (IsBitSet(modnumber, 8))
                modString += "HT";

            if (IsBitSet(modnumber, 3))
                modString += "HD";

            if (IsBitSet(modnumber, 4))
                modString += "HR";

            if (IsBitSet(modnumber, 6) && !IsBitSet(modnumber, 9))
                modString += "DT";

            if (IsBitSet(modnumber, 9))
                modString += "NC";

            if (IsBitSet(modnumber, 10))
                modString += "FL";

            if (IsBitSet(modnumber, 5))
                modString += "SD";

            if (IsBitSet(modnumber, 14))
                modString += "PF";

            if (IsBitSet(modnumber, 7))
                modString += "RX";

            if (IsBitSet(modnumber, 11))
                modString += "AT";

            if (IsBitSet(modnumber, 12))
                modString += "SO";

            return modString;

            static bool IsBitSet(int mods, int pos) => (mods & (1 << pos)) != 0;
        }

        public static string OsuGrade(string grade) => grade switch
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

        public static double OsuAccuracy(int count50, int count100, int count300, int countMiss) => ((double) 100 * ((50 * count50) + (100 * count100) + (300 * count300))) / (300 * (countMiss + count50 + count100 + count300));
    }
}