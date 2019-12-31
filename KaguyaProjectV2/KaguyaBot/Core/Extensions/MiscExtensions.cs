using System.Threading.Tasks;
using Discord;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;

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
            return 0;
        }
    }
}
