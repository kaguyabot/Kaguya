using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;

namespace KaguyaProjectV2.KaguyaBot.Core
{
    public static class Extensions
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
