using System;

namespace Kaguya.Core.Osu
{
    public enum OsuRequest
    {
        User,
        BestPerformance,
        RecentPlayed,
    }


    public static class OsuMisc
    {
        public static string ModeNames(int modnumber)
        {
            string modString;
            if (modnumber > 0)
            {
                modString = "+";
            }
            else
            {
                modString = "";
            }

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
                case "F":
                    return "<:F_:557297028263051288>"; 
            }
            return "";
        }

        public static double OsuAccuracy(int count50, int count100, int count300, int countMiss)
        {
            return 100 * ((50 * count50) + (100 * count100) + (300 * count300)) / ((300 * (countMiss + count50 + count100 + count300)));
        }

        public static string ToTimeAgo(TimeSpan time)
        {
            var timestamp = "";
            var year = 0;
            var month = 0;
            var day = time.Days;
            var hour = time.Hours;

            if (day >= 30)
            {
                while (true)
                {
                    month++;
                    day -= 30;
                    if (day < 30)
                    {
                        break;
                    }
                }
            }

            if (month >= 12)
            {
                while (true)
                {
                    month++;
                    month -= 12;
                    if (month < 12)
                    {
                        break;
                    }
                }
            }

            if (year > 1)
            {
                timestamp += $"{year} years ";
            }
            if (year == 1)
            {
                timestamp += $"{year} year ";
            }
            if (month > 1)
            {
                timestamp += $"{month} months ";
            }
            if (month == 1)
            {
                timestamp += $"{month} month ";
            }
            if (day > 1)
            {
                timestamp += $"{day} days and ";
            }
            if (day == 1)
            {
                timestamp += $"{day} day and ";
            }
            if (hour == 1)
            {
                timestamp += $"and {hour} hour ";
            }
            if (hour > 1)
            {
                timestamp += $"and {hour} hours ";
            }

            return timestamp;
        }
    }
}
