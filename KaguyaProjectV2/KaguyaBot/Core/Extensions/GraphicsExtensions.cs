using KaguyaProjectV2.KaguyaBot.Core.Images.ExpLevelUp;
using KaguyaProjectV2.KaguyaBot.Core.Images.Models;
using KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models;
using SixLabors.ImageSharp.Processing;
using PointF = SixLabors.Primitives.PointF;

namespace KaguyaProjectV2.KaguyaBot.Core.Extensions
{
    public static class GraphicsExtensions
    {
        /// <summary>
        /// An extension method to draw the given <see cref="TemplateText"/> onto the
        /// <see cref="IImageProcessingContext"/> that we're working with.
        /// <para>Usage: <code>SixLabors.ImageSharp.SomeImage.Mutate(x => x.DrawKaguyaText(...))</code></para>
        /// </summary>
        /// <param name="ctx">The <see cref="IImageProcessingContext"/> we are working with.</param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IImageProcessingContext DrawKaguyaText(this IImageProcessingContext ctx, TemplateText text)
        {
            if (text.Show)
            {
                var brush = new SolidBrush(text.Color);
                var stroke = new Pen(text.StrokeColor, text.StrokeWidth);

                return text.HasStroke
                    ? ctx.DrawText(text.Text, text.Font, brush, stroke, new PointF(text.Loc.X, text.Loc.Y))
                    : ctx.DrawText(text.Text, text.Font, text.Color, new PointF(text.Loc.X, text.Loc.Y));
            }

            return null;
        }

        public static IImageProcessingContext[] DrawKaguyaTemplatePanelText(this IImageProcessingContext ctx,
            ProfileTemplatePanel panel) => new IImageProcessingContext[]
        {
            ctx.DrawKaguyaText(panel.TopTextHeader),
            ctx.DrawKaguyaText(panel.TopTextBody),
            ctx.DrawKaguyaText(panel.BottomTextHeader),
            ctx.DrawKaguyaText(panel.BottomTextBody)
        };

        public static IImageProcessingContext[] DrawKaguyaXpPanelText(this IImageProcessingContext ctx,
            XpTemplate xpTemplate) => new IImageProcessingContext[]
        {
            ctx.DrawKaguyaText(xpTemplate.LevelText),
            ctx.DrawKaguyaText(xpTemplate.LevelUpMessageText),
            ctx.DrawKaguyaText(xpTemplate.NameText)
        };
    }
}