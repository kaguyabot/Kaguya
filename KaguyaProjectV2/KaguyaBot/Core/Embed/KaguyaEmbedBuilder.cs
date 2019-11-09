using Discord;

namespace KaguyaProjectV2.Core.Handlers
{
    public class KaguyaEmbedBuilder : EmbedBuilder
    {
        private static readonly Color RedColor = new Color(255, 0, 0);
        private static readonly Color VioletColor = new Color(111, 22, 255);
        private static readonly Color GoldColor = new Color(255, 223, 0);
        private static readonly Color BlueColor = new Color(56, 175, 255);
        private static readonly Color PinkColor = new Color(252, 132, 255);
        private static readonly Color GreenColor = new Color(0, 204, 0);

        public EmbedColor EmbedType;

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
        GREEN
    }
}