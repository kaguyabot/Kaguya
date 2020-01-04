using System;

namespace KaguyaProjectV2.KaguyaBot.Core.Extensions
{
    public static class MiscExtensions
    {
        public static bool IsZero(this int num)
        {
            if (num == 0)
                return true;
            return false;
        }

        public static int AsInteger(this string numString)
        {
            if (int.TryParse(numString, out int result))
                return result;
            throw new NullReferenceException("Could not parse string to int.");
        }

        public static ulong AsUlong(this string numString)
        {
            if (ulong.TryParse(numString, out ulong result))
                return result;
            throw new NullReferenceException("Could not parse string to ulong.");
        }
    }
}