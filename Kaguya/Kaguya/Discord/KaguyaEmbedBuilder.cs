using Discord;

namespace Kaguya.Discord
{
    public class KaguyaEmbedBuilder : EmbedBuilder
    {
        public KaguyaEmbedBuilder() => new KaguyaEmbedBuilder(global::Discord.Color.Blue);
        public KaguyaEmbedBuilder(Color color)
        {
            this.Color = color;
        }
    }
}