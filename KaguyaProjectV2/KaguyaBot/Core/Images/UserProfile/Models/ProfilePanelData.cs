using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Images.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models
{
    public class ProfilePanelData
    {
        private static float HeaderFontSize { get; } = 12;
        private static float BodyFontSize { get; } = 20;
        private static readonly Rgba32 _headerColor = Rgba32.WhiteSmoke;
        private static readonly Rgba32 _bodyColor = Rgba32.WhiteSmoke;

        public static async Task<ProfileTemplatePanel> LeftPanel(User user, Server server) => new ProfileTemplatePanel
        {
            TopTextHeader = new TemplateText
            {
                Color = _headerColor,
                Font = ImageBase.Font(HeaderFontSize),
                HasStroke = false,
                Loc = new TemplateLoc
                {
                    X = 182,
                    Y = 87
                },
                Show = true,
                Text = $"Global Rank"
            },
            TopTextBody = new TemplateText
            {
                Color = _bodyColor,
                Font = ImageBase.Font(BodyFontSize),
                HasStroke = false,
                Loc = new TemplateLoc
                {
                    X = 178,
                    Y = 98
                },
                Show = true,
                Text =
                    $"#{(await user.GetGlobalXpRankAsync()).Item1.ToAbbreviatedForm()} / {(await user.GetGlobalXpRankAsync()).Item2.ToAbbreviatedForm()}"
            },
            BottomTextHeader = new TemplateText
            {
                Color = _headerColor,
                Font = ImageBase.Font(HeaderFontSize),
                HasStroke = false,
                Loc = new TemplateLoc
                {
                    X = 175,
                    Y = 115
                },
                Show = true,
                Text = "Server Rank"
            },
            BottomTextBody = new TemplateText
            {
                Color = _bodyColor,
                Font = ImageBase.Font(BodyFontSize),
                HasStroke = false,
                Loc = new TemplateLoc
                {
                    X = 171,
                    Y = 126
                },
                Show = true,
                Text = $"#{user.GetServerXpRank(server).Item1.ToAbbreviatedForm()} / {user.GetServerXpRank(server).Item2.ToAbbreviatedForm()}"
            }
        };

        public static ProfileTemplatePanel RightPanel(User user) => new ProfileTemplatePanel
        {
            TopTextHeader = new TemplateText
            {
                Color = _headerColor,
                Font = ImageBase.Font(HeaderFontSize),
                HasStroke = false,
                Loc = new TemplateLoc
                {
                    X = 368,
                    Y = 87
                },
                Show = true,
                Text = $"Points"
            },
            TopTextBody = new TemplateText
            {
                Color = _headerColor,
                Font = ImageBase.Font(BodyFontSize),
                HasStroke = false,
                Loc = new TemplateLoc
                {
                    X = 364,
                    Y = 99
                },
                Show = true,
                Text = $"{user.Points.ToAbbreviatedForm()}"
            },
            BottomTextHeader = new TemplateText
            {
                Color = _headerColor,
                Font = ImageBase.Font(HeaderFontSize),
                HasStroke = false,
                Loc = new TemplateLoc
                {
                    X = 362,
                    Y = 116
                },
                Show = true,
                Text = $"Rep"
            },
            BottomTextBody = new TemplateText
            {
                Color = _headerColor,
                Font = ImageBase.Font(BodyFontSize),
                HasStroke = false,
                Loc = new TemplateLoc
                {
                    X = 363,
                    Y = 127
                },
                Show = true,
                Text = $"{user.Rep.Count().ToAbbreviatedForm()}"
            }
        };
    }
}