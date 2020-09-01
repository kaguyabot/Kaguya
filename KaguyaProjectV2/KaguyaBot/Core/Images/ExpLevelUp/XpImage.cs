using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Images.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Image = SixLabors.ImageSharp.Image;
// ReSharper disable AccessToDisposedClosure

namespace KaguyaProjectV2.KaguyaBot.Core.Images.ExpLevelUp
{
    public class XpImage : ImageBase
    {
        public async Task<Stream> GenerateXpImageStream(User user, SocketGuildUser guildUser, Server server = null)
        {
            var templateStream = new MemoryStream();
            MemoryStream pfpStream;
            MemoryStream badgeStream;

            var xp = new XpTemplate
            {
                ProfilePicture = new TemplateIcon
                {
                    Loc = new TemplateLoc
                    {
                        X = 13,
                        Y = 15
                    },
                    ProfileUrl = guildUser.GetAvatarUrl(),
                    Show = true
                },
                LevelUpMessageText = new TemplateText
                {
                    Color = Rgba32.WhiteSmoke,
                    Font = Font(server is null ? 60 : 48),
                    HasStroke = true,
                    Loc = server is null
                    ? new TemplateLoc
                    {
                        X = 210,
                        Y = 125
                    } : new TemplateLoc
                    {
                        X = 160,
                        Y = 125
                    },
                    Show = true,
                    StrokeColor = Rgba32.LightSalmon,
                    StrokeWidth = 1.4f,
                    Text = $"{(server is null ? "Level Up!" : "Server Level Up!")}"
                },
                NameText = new TemplateText
                {
                    Color = Rgba32.WhiteSmoke,
                    Font = Font(guildUser.Username.Length > 23 ? guildUser.Username[..23] : guildUser.Username, 65, 5),
                    HasStroke = true,
                    Loc = new TemplateLoc
                    {
                        X = XpNameXCoordinate(guildUser.Username.Length > 23 ? guildUser.Username[..23] : guildUser.Username),
                        Y = 68
                    },
                    Show = true,
                    StrokeColor = Rgba32.Red,
                    StrokeWidth = 1.4f,
                    Text = guildUser.Username.Length > 23 ? guildUser.Username[..23] : guildUser.Username
                },
                LevelText = new TemplateText
                {
                    Color = Rgba32.WhiteSmoke,
                    Font = Font(50),
                    HasStroke = true,
                    Loc = new TemplateLoc
                    {
                        X = user.GlobalLevel().Rounded(RoundDirection.Down).ToString().Length == 1
                            ? 300 : user.GlobalLevel().Rounded(RoundDirection.Down).ToString().Length > 2 ? 270 : 288,
                        Y = 200
                    },
                    Show = true,
                    StrokeColor = Rgba32.DarkSalmon,
                    StrokeWidth = 1,
                    Text = server is null
                        ? user.GlobalLevel().Rounded(RoundDirection.Down).ToString()
                        : user.ServerLevel(server).Rounded(RoundDirection.Down).ToString()
                },
                SupporterBadge = new TemplateBadge
                {
                    Emote = Emote.Parse("<:KaguyaSupporter:672187970534768641>"),
                    Loc = new TemplateLoc
                    {
                        X = 94,
                        Y = 94
                    },
                    User = user
                }
            };

            using (var wc = new WebClient())
            {
                string pfpLink = xp.ProfilePicture?.ProfileUrl ?? guildUser.GetDefaultAvatarUrl();

                var pfpImg = await wc.DownloadDataTaskAsync(pfpLink);
                pfpStream = new MemoryStream(pfpImg);

                var badgeImg = await wc.DownloadDataTaskAsync(xp.SupporterBadge.Emote.Url);
                badgeStream = new MemoryStream(badgeImg);
            }

            using var image = Image.Load(XP_TEMPLATE_PATH);
            using var canvas = new Image<Rgba32>(image.Width, image.Height);
            using var profilePicture = Image.Load(pfpStream);
            using var suppBadge = Image.Load(badgeStream);

            const double resizeScalar = 0.82;
            suppBadge.Mutate(x => x.Resize((int)(suppBadge.Width * resizeScalar), (int)(suppBadge.Height * resizeScalar)));

            if (xp.ProfilePicture.ProfileUrl == null)
            {
                profilePicture.Mutate(x => x.Resize(132, 132));
            }
            // The reason why we draw the template onto the profile picture here is because 
            // the profile picture is beneath the template. Our template contains a cutout for this, 
            // so we draw everything ontop of our virtual base layer (in this case, our profile pic).
            //
            // More documentation can be found in ProfileImage.cs
            //
            // Unlike ProfileImage.cs, however, I decided to go with a deliberate blank canvas to draw ontop of.
            canvas.Mutate(x => x.DrawImage(profilePicture, new Point((int)xp.ProfilePicture.Loc.X, (int)xp.ProfilePicture.Loc.Y), 1));
            canvas.Mutate(x => x.DrawImage(image, 1));
            canvas.Mutate(x => x.DrawKaguyaXpPanelText(xp));

            if (user.IsPremium)
            {
                canvas.Mutate(x => x.DrawImage(suppBadge, new Point((int)xp.SupporterBadge.Loc.X, (int)xp.SupporterBadge.Loc.Y), 1));
            }

            canvas.SaveAsPng(templateStream);
            templateStream.Seek(0, SeekOrigin.Begin);
            return templateStream;
        }
    }
}
