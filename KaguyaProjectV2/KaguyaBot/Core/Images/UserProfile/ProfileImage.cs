using KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.GraphicsData;
using KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using SixLabors.Primitives;
using Image = SixLabors.ImageSharp.Image;
// ReSharper disable AccessToDisposedClosure

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile
{
    public class ProfileImage
    {
        /// <summary>
        /// Generates the profile image for the provided <see cref="User"/>. A <see cref="Server"/>
        /// is passed in to display their Server EXp progress and rank.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public async Task<Stream> GenerateProfileImageStream(User user, Server server, SocketGuildUser guildUser)
        {
            var templateStream = new MemoryStream();
            MemoryStream pfpStream;
            MemoryStream badgeStream;

            var profile = new ProfileTemplate
            {
                Xp = new ProfileTemplateXp
                {
                    GlobalBar = GlobalBarData.Bar(user),
                    GuildBar = GuildBarData.Bar,
                    IconAndUsername = ProfilePictureData.ProfileIcon(guildUser),
                    SupporterBadge = new SupporterBadge(user).Data
                }
            };

            var pfpIcon = ProfilePictureData.ProfileIcon(guildUser);

            using (var wc = new WebClient())
            {
                var pfpImg = await wc.DownloadDataTaskAsync(guildUser.GetAvatarUrl(ImageFormat.Png));
                pfpStream = new MemoryStream(pfpImg);

                var badgeImg = await wc.DownloadDataTaskAsync(profile.Xp.SupporterBadge.Emote.Url);
                badgeStream = new MemoryStream(badgeImg);
            }

            using var image = Image.Load(GraphicsConstants.TEMPLATE_PATH);
            using var profilePicture = Image.Load(pfpStream);
            using var suppBadge = Image.Load(badgeStream);
            using var gBar = new Image<Rgba32>(image.Width, image.Height);
            using var sBar = new Image<Rgba32>(image.Width, image.Height);

            var gFillPoints = GlobalBarData.GlobalXpBarCoordinates(user, profile.Xp, new ProfileTemplateUserData(user, server));

            /*
                 * In order to avoid clipping and artifacts, we render a completely new image
                 * that just contains the bars themselves. This serves as our "layer zero" if
                 * we were in Photoshop.
                 *
                 * Now that we have our base layer, we draw the template on top as our
                 * middle layer. This is because the template has transparent "cutouts" for
                 * our bars to fit into. In order for the bar to look pretty, the template must
                 * "trim" the bar for us naturally. This is why we have to draw the template on top
                 * separately.
                 *
                 * We do this for all bars and images that are filling transparent areas on our template.
            */

            // Resize downloaded supporter image, she's a little too chonky <(^-^)>
            double resizeScalar = 0.75;
            suppBadge.Mutate(x => x.Resize((int)(x.GetCurrentSize().Width * resizeScalar), (int)(x.GetCurrentSize().Height * resizeScalar)));

            // Draw the profile picture on top of the global bar. Global bar will serve as the base layer 
            // that we continually add onto, even if it doesn't directly overlap it, as it's a layer 
            // with the same size of our template. Think of it as a canvas.
            gBar.Mutate(x => x.DrawImage(profilePicture, new Point(12, 12), 1));
            gBar.Mutate(x => x.FillPolygon(profile.Xp.GlobalBar.Color, gFillPoints));
            gBar.Mutate(x => x.DrawImage(image, 1));

            gBar.Mutate(x => x.DrawKaguyaText(pfpIcon.UsernameText));
            gBar.Mutate(x => x.DrawKaguyaText(pfpIcon.UserDiscriminatorText));

            gBar.Mutate(x => x.DrawKaguyaText(profile.Xp.GlobalBar.TopLeftText));
            gBar.Mutate(x => x.DrawKaguyaText(profile.Xp.GlobalBar.BottomRightText));
            gBar.Mutate(x => x.DrawKaguyaText(profile.Xp.GlobalBar.CenterText));

            // Draw supporter badge.
            if (user.IsSupporter)
            {
                gBar.Mutate(x => x.DrawImage(suppBadge,
                    new Point((int)profile.Xp.SupporterBadge.Loc.X, (int)profile.Xp.SupporterBadge.Loc.Y), 1));
            }

            gBar.SaveAsPng(templateStream);
            templateStream.Seek(0, SeekOrigin.Begin);
            return templateStream;
        }
    }
}
