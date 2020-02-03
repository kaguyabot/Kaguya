using KaguyaProjectV2.KaguyaBot.Core.Images.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.ExpLevelUp
{
    public class XpTemplate : ImageBase
    {
        public TemplateIcon ProfilePicture { get; set; }
        public TemplateText LevelUpMessageText { get; set; }
        public TemplateText NameText { get; set; }
        public TemplateText LevelText { get; set; }
        public TemplateBadge SupporterBadge { get; set; }
    }
}