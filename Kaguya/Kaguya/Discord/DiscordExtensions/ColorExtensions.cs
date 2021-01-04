using System.Globalization;
using Kaguya.Exceptions;

namespace Kaguya.Discord.DiscordExtensions
{
    public static class ColorExtensions
    {
        public static uint ToHexadecimal(this string hexColor)
        {
            if (uint.TryParse(hexColor, NumberStyles.HexNumber, null, out uint hexadecimal))
            {
                return hexadecimal;
            }

            throw new ColorParseException($"Failed to convert text \"{hexColor}\" into hexadecimal value.");
        }
    }
}