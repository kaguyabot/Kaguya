using System.Collections.Generic;
using SixLabors.ImageSharp.PixelFormats;

// ReSharper disable AccessToDisposedClosure

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models
{
    /*
     * I primarily created these classes so that it would be very easy to change the
     * location of texts/bars/images' locations in the event that the template were to change.
     * It would be as simple as editing the X-Y coordinates of the objects.
     */

    public class ProfileTemplateXpBar
    {
        /// <summary>
        /// The color of the bar's fill.
        /// </summary>
        public Rgba32 Color { get; set; }
        /// <summary>
        /// The length of the bar, in pixels.
        /// </summary>
        public int Length { get; set; }
        public ProfileTemplateLoc LocA { get; set; }
        public ProfileTemplateLoc LocB { get; set; }
        public ProfileTemplateText TopLeftText { get; set; }
        public ProfileTemplateText BottomRightText { get; set; }
        public ProfileTemplateText CenterText { get; set; }
    }
}
