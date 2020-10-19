namespace KaguyaProjectV2.KaguyaBot.Core.Images.Models
{
    public class TemplateIcon
    {
        public TemplateLoc Loc { get; set; }
        public string ProfileUrl { get; set; }
        public bool Show { get; set; }
        public TemplateText UsernameText { get; set; }
        public TemplateText UserDiscriminatorText { get; set; }
    }
}