using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.Models
{
    public class TemplateText
    {
        public Rgba32 Color { get; set; } = Rgba32.White;
        public TemplateLoc Loc { get; set; }
        public Font Font { get; set; }
        public string Text { get; set; }
        public bool Show { get; set; } = true;
        public bool HasStroke { get; set; }
        public float StrokeWidth { get; set; }
        public Rgba32 StrokeColor { get; set; } = Rgba32.Black;
    }
}