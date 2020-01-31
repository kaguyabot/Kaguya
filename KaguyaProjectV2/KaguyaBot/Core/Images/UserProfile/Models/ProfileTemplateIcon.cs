// ReSharper disable AccessToDisposedClosure

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models
{
    public class ProfileTemplateIcon
    {
        public ProfileTemplateLoc Loc { get; set; }
        public string ProfileUrl { get; set; }
        public bool Show { get; set; }
        public ProfileTemplateText UsernameText { get; set; }
        public ProfileTemplateText UserDiscriminatorText { get; set; }
    }
}
