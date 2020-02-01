using System;
using System.Collections.Generic;
using System.Text;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models
{
    public class ProfileTemplatePanel
    {
        /// <summary>
        /// The small print at the top of the top text's data, this acts as a header.
        /// </summary>
        public ProfileTemplateText TopTextHeader { get; set; }
        public ProfileTemplateText TopTextBody { get; set; }
        public ProfileTemplateText BottomTextHeader { get; set; }
        public ProfileTemplateText BottomTextBody { get; set; }
    }
}
