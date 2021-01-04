using Discord;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Exceptions;

namespace Kaguya.Discord
{
    public class KaguyaEmbedBuilder : EmbedBuilder
    {
        public KaguyaEmbedBuilder() { this.Color ??= global::Discord.Color.Blue; }
        /// <summary>
        /// Converts the given hexadecimal color string into a <see cref="Color"/>.
        ///
        /// Example: new KaguyaEmbedBuilder("FFFF99") { }
        /// </summary>
        /// <param name="hex"></param>
        /// <exception cref="ColorParseException">Thrown if the hexadecimal string could not be parsed as a uint.</exception>
        public KaguyaEmbedBuilder(string hex) : this(new Color(hex.ToHexadecimal()))
        { }

        public KaguyaEmbedBuilder(Color color)
        {
            this.Color = color;
        }
    }
}