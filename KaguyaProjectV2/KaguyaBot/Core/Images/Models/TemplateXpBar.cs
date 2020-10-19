using SixLabors.ImageSharp.PixelFormats;

// ReSharper disable AccessToDisposedClosure
namespace KaguyaProjectV2.KaguyaBot.Core.Images.Models
{
    /*
     * I primarily created these classes so that it would be very easy to change the
     * location of texts/bars/images' locations in the event that the template were to change.
     * It would be as simple as editing the X-Y coordinates of the objects.
     */

    /// <summary>
    /// Represents a generic rectange with two <see cref="TemplateLoc"/>s that determine where the bar shall fill out from.
    /// </summary>
    public class TemplateXpBar
    {
        /// <summary>
        /// The color of the bar's fill.
        /// </summary>
        public Rgba32 Color { get; set; }

        /// <summary>
        /// The length of the bar, in pixels.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// The top-left coordinate of this bar.
        /// </summary>
        public TemplateLoc LocA { get; set; }

        /// <summary>
        /// The bottom-left coordinate of this bar.
        /// </summary>
        public TemplateLoc LocB { get; set; }

        public TemplateText TopLeftText { get; set; }
        public TemplateText BottomRightText { get; set; }
        public TemplateText CenterText { get; set; }
    }
}