using KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace KaguyaProjectV2.KaguyaBot.Core.Extensions
{
    public static class GraphicsExtensions
    {
        /// <summary>
        /// An extension method to draw the given <see cref="ProfileTemplateText"/> onto the
        /// <see cref="IImageProcessingContext"/> that we're working with.
        /// <para>Usage: <code>SixLabors.ImageSharp.SomeImage.Mutate(x => x.DrawKaguyaText(...))</code></para>
        /// </summary>
        /// <param name="ctx">The <see cref="IImageProcessingContext"/> we are working with.</param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IImageProcessingContext DrawKaguyaText(this IImageProcessingContext ctx, ProfileTemplateText text)
        {
            if(text.Show)
                return ctx.DrawText(text.Text, text.Font, text.Color, new PointF(text.Loc.X, text.Loc.Y));
            return null;
        }
    }
}
