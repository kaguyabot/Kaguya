using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Discord;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Commands.Currency;
using KaguyaProjectV2.KaguyaBot.Core.Commands.Currency.Poker;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

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

        public static int AsInteger(this string numString, bool throwException = true)
        {
            if (int.TryParse(numString, out int result))
                return result;
            if (throwException)
                throw new NullReferenceException("Could not parse string to integer.");
            return 0;
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
        /// Ex: 1000 => 1.00K, 525500 => 525.5K, 1250000 => 1.25M
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ToAbbreviatedForm(this int num)
        {
            return num > 1000000
                ? $"{((double)num / 1000000):N2}M"
                : num > 100000
                    ? $"{((double)num / 1000):N1}K"
                    : num > 1000
                        ? $"{((double)num / 1000):N2}K"
                        : num.ToString();
        }

        public static bool ContainsEmoji(this string text)
        {
            Regex rgx = new Regex(@"[\uD83C-\uDBFF\uDC00-\uDFFF]+");
            return rgx.IsMatch(text);
        }

        /// <summary>
        /// Alters a string that may contain an emoji and removes all of them, if they are present.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FilterEmojis(this string text)
        {
            return text.Where(c => !c.ToString().ContainsEmoji()).Aggregate("", (current, c) => current + c);
        }

        public static int Rounded(this double num, RoundDirection dir)
        {
            return dir switch
            {
                RoundDirection.Down => (int)Math.Floor(num),
                RoundDirection.Up => (int)Math.Ceiling(num),
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public static bool IsRedeemed(this PremiumKey key)
        {
            return key.UserId != 0 || key.ServerId != 0;
        }
        
        public static string ToReadable(this IEnumerable<Card> cards)
        {
            return cards.Humanize(x => x.ToString(), "").Replace(",", "");
        }
    }

    public enum RoundDirection
    {
        Down,
        Up
    }
}