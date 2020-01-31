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
        /// is passed in to display their Server EXP progress and rank.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public async Task<Stream> GenerateProfileImageStream(User user, Server server, SocketGuildUser guildUser)
        {
            var templateStream = new MemoryStream();
            MemoryStream pfpStream;

            var xp = new ProfileTemplateXp
            {
                GlobalBar = GlobalBarData.Bar(user),
                GuildBar = GuildBarData.Bar
            };

            var pfpIcon = ProfilePictureData.ProfileIcon(guildUser);

            using (var wc = new WebClient())
            {
                var pfpImg = await wc.DownloadDataTaskAsync(guildUser.GetAvatarUrl(ImageFormat.Png));
                pfpStream = new MemoryStream(pfpImg);
            }

            using var image = Image.Load(GraphicsConstants.TEMPLATE_PATH);
            using var profilePicture = Image.Load(pfpStream);
            using var gBar = new Image<Rgba32>(image.Width, image.Height);
            using var sBar = new Image<Rgba32>(image.Width, image.Height);

            var gFillPoints = GlobalBarData.GlobalXpBarCoordinates(user, xp, new ProfileTemplateUserData(user, server));

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

            gBar.Mutate(x => x.DrawImage(profilePicture, new Point(12, 12), 1));
            gBar.Mutate(x => x.FillPolygon(xp.GlobalBar.Color, gFillPoints));
            gBar.Mutate(x => x.DrawImage(image, 1));

            gBar.Mutate(x => x.DrawKaguyaText(pfpIcon.UsernameText));
            gBar.Mutate(x => x.DrawKaguyaText(pfpIcon.UserDiscriminatorText));

            gBar.Mutate(x => x.DrawKaguyaText(xp.GlobalBar.TopLeftText));
            gBar.Mutate(x => x.DrawKaguyaText(xp.GlobalBar.BottomRightText));
            gBar.Mutate(x => x.DrawKaguyaText(xp.GlobalBar.CenterText));

            gBar.SaveAsPng(templateStream);
            templateStream.Seek(0, SeekOrigin.Begin);
            return templateStream;
        }
    }
}
