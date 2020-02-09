using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Global;

namespace KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed
{
    public class KaguyaEmbedBuilder : EmbedBuilder
    {
        public static readonly Color RedColor = new Color(255, 0, 0);
        public static readonly Color VioletColor = new Color(111, 22, 255);
        public static readonly Color GoldColor = new Color(255, 223, 0);
        public static readonly Color BlueColor = new Color(56, 175, 255);
        public static readonly Color PinkColor = new Color(252, 132, 255);
        public static readonly Color GreenColor = new Color(0, 204, 0);
        public static readonly Color OrangeColor = new Color(245, 150, 34);
        public static readonly Color YellowColor = new Color(250, 246, 30);
        public static readonly Color BlackColor = new Color(0, 0, 0);
        public static readonly Color LightPurpleColor = new Color(189, 74, 255);
        public static readonly Color LightBlueColor = new Color(82, 229, 255);
        public static readonly Color GrayColor = new Color(117, 117, 117);

        public KaguyaEmbedBuilder()
        {
            SetColor();
        }

        public KaguyaEmbedBuilder(EmbedColor type)
        {
            SetColor(type);
        }

        public void SetColor(EmbedColor type = EmbedColor.BLUE)
        {
            Color color;
            switch (type)
            {
                case EmbedColor.VIOLET:
                    color = VioletColor;
                    break;
                case EmbedColor.GOLD:
                    color = GoldColor;
                    break;
                case EmbedColor.PINK:
                    color = PinkColor;
                    break;
                case EmbedColor.RED:
                    color = RedColor;
                    break;
                case EmbedColor.BLUE:
                    color = BlueColor;
                    break;
                case EmbedColor.GREEN:
                    color = GreenColor;
                    break;
                case EmbedColor.YELLOW:
                    color = YellowColor;
                    break;
                case EmbedColor.BLACK:
                    color = BlackColor;
                    break;
                case EmbedColor.LIGHT_PURPLE:
                    color = LightPurpleColor;
                    break;
                case EmbedColor.LIGHT_BLUE:
                    color = LightBlueColor;
                    break;
                case EmbedColor.ORANGE:
                    color = OrangeColor;
                    break;
                case EmbedColor.GRAY:
                    color = GrayColor;
                    break;
                default:
                    color = RedColor;
                    break;
            }
            WithColor(color);
        }
    }

    public enum EmbedColor
    {
        RED,
        VIOLET,
        GOLD,
        BLUE,
        PINK,
        GREEN,
        YELLOW,
        BLACK,
        LIGHT_PURPLE,
        LIGHT_BLUE,
        ORANGE,
        GRAY
    }
}