using Discord;

namespace Kaguya.Discord.Embeds
{
    public class KaguyaEmbedBuilder : EmbedBuilder
    {
        public static readonly Color RedColor = new (255, 0, 0);
        public static readonly Color VioletColor = new (111, 22, 255);
        public static readonly Color GoldColor = new (255, 223, 0);
        public static readonly Color BlueColor = new (56, 175, 255);
        public static readonly Color PinkColor = new (252, 132, 255);
        public static readonly Color GreenColor = new (0, 204, 0);
        public static readonly Color OrangeColor = new (245, 150, 34);
        public static readonly Color YellowColor = new (250, 246, 30);
        public static readonly Color BlackColor = new (0, 0, 0);
        public static readonly Color LightPurpleColor = new (189, 74, 255);
        public static readonly Color LightBlueColor = new (82, 229, 255);
        public static readonly Color GrayColor = new (117, 117, 117);
        public static readonly Color MagentaColor = new (235, 44, 140);

        public KaguyaEmbedBuilder() => new KaguyaEmbedBuilder(BlueColor);
        public KaguyaEmbedBuilder(Color color)
        {
            this.Color = color;
        }
    }
}