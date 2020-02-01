// ReSharper disable AccessToDisposedClosure

using KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.GraphicsData;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models
{
    public class ProfileTemplateXp
    {
        public ProfileTemplateXpBar GlobalBar { get; set; }
        public ProfileTemplateXpBar GuildBar { get; set; }
        public ProfileTemplateBadge SupporterBadge { get; set; }
        public ProfileTemplateIcon IconAndUsername { get; set; }
        public ProfileTemplatePanel LeftPanel { get; set; }
        public ProfileTemplatePanel RightPanel { get; set; }
    }
}
