using System;
using System.Collections.Immutable;
using Discord;

namespace Kaguya.Core.Embed
{
    public class KaguyaEmbedBuilder : EmbedBuilder
    {
        private static readonly Color RedColor = new Color(255, 0, 0);
        private static readonly Color VioletColor = new Color(238, 130, 238);
        private static readonly Color GoldColor = new Color(255, 223, 0);
        private static readonly Color PinkColor = new Color(252, 132, 255);

        public EmbedType EmbedType;

        public KaguyaEmbedBuilder()
        {
            SetColor();
        }

        public KaguyaEmbedBuilder(EmbedType type)
        {
            SetColor(type);
        }

        public void SetColor(EmbedType type = EmbedType.PINK)
        {
            Color color;
            switch (type)
            {
                case EmbedType.VIOLET:
                    color = VioletColor;
                    break;
                case EmbedType.GOLD:
                    color = GoldColor;
                    break;
                case EmbedType.PINK:
                    color = PinkColor;
                    break;
                case EmbedType.RED:
                    color = RedColor;
                    break;
                default:
                    color = RedColor;
                    break;
            }
            WithColor(color);
        }
    }

    public enum EmbedType
    {
        RED,
        VIOLET,
        GOLD,
        PINK
    }
}
