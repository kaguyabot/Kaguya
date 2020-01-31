using System;
using System.Numerics;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Shapes;

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

        /// <summary>
        /// Attempts to convert the given string into a ulong.
        /// </summary>
        /// <param name="numString"></param>
        /// <param name="throwExceptionIfNull"></param>
        /// <returns></returns>
        public static ulong AsUlong(this string numString, bool throwExceptionIfNull = true)
        {
            if (ulong.TryParse(numString, out ulong result))
                return result;
            if (throwExceptionIfNull)
                throw new NullReferenceException("Could not parse string to ulong.");
            return 0;
        }

        /// <summary>
        /// Convertes the provided integer into it's abbreviated word form.
        /// Ex: 1000 => 1K, 525500 => 525.5K, 1250000 => 1.25M
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ToAbbreviatedForm(this int num)
        {
            return num > 1000000 
                ? $"{(num / 1000000):N2}M" 
                : num > 1000 
                    ? $"{((double)num / 1000):N2}K" 
                    : num.ToString();
        }

        public static bool ContainsEmoji(this string text)
        {
            Regex rgx = new Regex(@"[\uD83C-\uDBFF\uDC00-\uDFFF]+");
            return rgx.IsMatch(text);
        }
    }
}