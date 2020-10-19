using KaguyaProjectV2.KaguyaBot.Core.Images.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models
{
    public class ProfileTemplateXp
    {
        public TemplateXpBar GlobalBar { get; set; }
        public TemplateXpBar GuildBar { get; set; }
        public TemplateBadge PremiumBadge { get; set; }
        public TemplateIcon IconAndUsername { get; set; }
        public ProfileTemplatePanel LeftPanel { get; set; }
        public ProfileTemplatePanel RightPanel { get; set; }
    }
}