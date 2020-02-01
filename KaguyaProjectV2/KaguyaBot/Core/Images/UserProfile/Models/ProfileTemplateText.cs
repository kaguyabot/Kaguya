using KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
// ReSharper disable AccessToDisposedClosure

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models
{
    public class ProfileTemplateText
    {
        public Rgba32 Color { get; set; } = Rgba32.White;
        public ProfileTemplateLoc Loc { get; set; }
        public Font Font { get; set; }
        public string Text { get; set; }
        public bool Show { get; set; } = true;
        public bool HasStroke { get; set; }
        public float StrokeWidth { get; set; }
        public Rgba32 StrokeColor { get; set; } = Rgba32.Black;
    }
}
