using System;

namespace Kaguya.Internal.PrimitiveExtensions
{
    public static class MathExtensions
    {
        public static int ToFloor(this double d) => (int) Math.Floor(d);
        public static int ToFloor(this decimal d) => (int) Math.Floor(d);
        /// <summary>
        /// Formats the <see cref="num"/> into a format as follows:
        ///
        /// (one billion) => 1B
        /// 1000000 => 1M
        /// 1000 => 1K
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ToShorthandFormat(this long num)
        {
            long i = (long)Math.Pow(10, (long)Math.Max(0, Math.Log10(num) - 2));
            num = num / i * i;

            if (num >= 1000000000)
                return (num / 1000000000D).ToString("0.##") + "B";
            if (num >= 1000000)
                return (num / 1000000D).ToString("0.##") + "M";
            if (num >= 1000)
                return (num / 1000D).ToString("0.##") + "K";

            return num.ToString("#,0");
        }

        // This call is necessary because ints and longs invoke different extension methods.
        public static string ToShorthandFormat(this int num) => ToShorthandFormat((long) num);
    }
}