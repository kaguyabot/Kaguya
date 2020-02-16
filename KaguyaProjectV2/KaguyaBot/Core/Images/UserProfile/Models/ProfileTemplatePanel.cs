using KaguyaProjectV2.KaguyaBot.Core.Images.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models
{
    public class ProfileTemplatePanel
    {
        /// <summary>
        /// The small print at the top of the top text's data, this acts as a header.
        /// </summary>
        public TemplateText TopTextHeader { get; set; }
        public TemplateText TopTextBody { get; set; }
        public TemplateText BottomTextHeader { get; set; }
        public TemplateText BottomTextBody { get; set; }
    }
}
