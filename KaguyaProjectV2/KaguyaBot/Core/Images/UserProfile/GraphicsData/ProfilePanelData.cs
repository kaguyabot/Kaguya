using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.GraphicsData
{
    public class ProfilePanelData
    {
        private static float HeaderFontSize { get; } = 12;
        private static float BodyFontSize { get; } = 20;
        private static readonly Rgba32 HeaderColor = Rgba32.WhiteSmoke;
        private static readonly Rgba32 BodyColor = Rgba32.WhiteSmoke;

        public static async Task<ProfileTemplatePanel> LeftPanel(User user, Server server)
        {
            return new ProfileTemplatePanel
            {
                TopTextHeader = new ProfileTemplateText
                {
                    Color = HeaderColor,
                    Font = GraphicsConstants.Font(HeaderFontSize),
                    HasStroke = false,
                    Loc = new ProfileTemplateLoc
                    {
                        X = 182,
                        Y = 87
                    },
                    Show = true,
                    Text = $"Global Rank"
                },
                TopTextBody = new ProfileTemplateText
                {
                    Color = BodyColor,
                    Font = GraphicsConstants.Font(BodyFontSize),
                    HasStroke = false,
                    Loc = new ProfileTemplateLoc
                    {
                        X = 178,
                        Y = 98
                    },
                    Show = true,
                    Text = $"#{(await user.GetGlobalXpRank()).Item1.ToAbbreviatedForm()} / {(await user.GetGlobalXpRank()).Item2.ToAbbreviatedForm()}"
                },
                BottomTextHeader = new ProfileTemplateText
                {
                    Color = HeaderColor,
                    Font = GraphicsConstants.Font(HeaderFontSize),
                    HasStroke = false,
                    Loc = new ProfileTemplateLoc
                    {
                        X = 175,
                        Y = 115
                    },
                    Show = true,
                    Text = "Server Rank"
                },
                BottomTextBody = new ProfileTemplateText
                {
                    Color = BodyColor,
                    Font = GraphicsConstants.Font(BodyFontSize),
                    HasStroke = false,
                    Loc = new ProfileTemplateLoc
                    {
                        X = 171,
                        Y = 126
                    },
                    Show = true,
                    Text = $"#{user.GetServerXpRank(server).Item1.ToAbbreviatedForm()} / {user.GetServerXpRank(server).Item2.ToAbbreviatedForm()}"
                }
            };
        }

        public static async Task<ProfileTemplatePanel> RightPanel(User user)
        {
            return new ProfileTemplatePanel
            {
                TopTextHeader = new ProfileTemplateText
                {
                    Color = HeaderColor,
                    Font = GraphicsConstants.Font(HeaderFontSize),
                    HasStroke = false,
                    Loc = new ProfileTemplateLoc
                    {
                        X = 368,
                        Y = 87
                    },
                    Show = true,
                    Text = $"Points"
                },
                TopTextBody = new ProfileTemplateText
                {
                    Color = HeaderColor,
                    Font = GraphicsConstants.Font(BodyFontSize),
                    HasStroke = false,
                    Loc = new ProfileTemplateLoc
                    {
                        X = 364,
                        Y = 99
                    },
                    Show = true,
                    Text = $"{user.Points.ToAbbreviatedForm()}"
                },
                BottomTextHeader = new ProfileTemplateText
                {
                    Color = HeaderColor,
                    Font = GraphicsConstants.Font(HeaderFontSize),
                    HasStroke = false,
                    Loc = new ProfileTemplateLoc
                    {
                        X = 362,
                        Y = 116
                    },
                    Show = true,
                    Text = $"Rep"
                },
                BottomTextBody = new ProfileTemplateText
                {
                    Color = HeaderColor,
                    Font = GraphicsConstants.Font(BodyFontSize),
                    HasStroke = false,
                    Loc = new ProfileTemplateLoc
                    {
                        X = 363,
                        Y = 127
                    },
                    Show = true,
                    Text = $"{user.RepCount.ToAbbreviatedForm()}"
                }
            };
        }
    }
}
