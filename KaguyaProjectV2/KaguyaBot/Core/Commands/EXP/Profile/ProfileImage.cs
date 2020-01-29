using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Color = SixLabors.ImageSharp.Color;
using Image = SixLabors.ImageSharp.Image;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP.Profile
{
    public class ProfileImage
    {
        public async Task<Stream> GenerateImageStream()
        {
            MemoryStream stream = new MemoryStream();

            // Todo: Un-hardcode this.
            // Todo: Fix errors and properly import font.
            // const string imagePath = $@"{ConfigProperties.KaguyaMainFolder}\Resources\Images\ProfileSmall.png";
            const string imagePath = @"C:\Users\stage\Desktop\Artwork\KaguyaArtworkAssets\Discord-Chat-Images\ProfileSmall.png";
            const string fontPath = @"C:\Users\stage\Documents\GitHub\Kaguya\KaguyaProjectV2\Resources\Fonts\frankMedium.ttf";
            var fontCollection = new FontCollection();
            FontFamily frankGothicFont = fontCollection.Install(fontPath);
            var font = new Font(frankGothicFont, 14);

            using (Image image = Image.Load(imagePath))
            {
                image.Mutate(x => x.DrawText($"1234", font, Color.DarkOrange, Loc(100, 100)));

                image.SaveAsPng(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
        }

        private PointF Loc(float x, float y)
        {
            return new PointF(x, y);
        }
    }

    public class ProfileTemplateLoc
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class ProfileTemplateText
    {
        public Rgba32 Color { get; set; } = Rgba32.White;
        public bool Show { get; set; } = true;
        public ProfileTemplateLoc Loc { get; set; }

    }

    public class ProfileTemplateIcon
    {
        public bool Show { get; set; }
        public ProfileTemplateLoc Loc { get; set; }
    }

    public class UserProfileData
    {
        private SocketGuildUser User { get; }
        public UserProfileData(User user, Server server)
        {
            User = ConfigProperties.Client.GetGuild(server.ServerId).GetUser(user.UserId);
            ServerXp = server.ServerExp.First(x => x.UserId == user.UserId).Exp;
            GlobalXp = user.Experience;
            Username = User.Username;
            Discriminator = User.Discriminator;
            ProfileUrl = User.GetAvatarUrl();
            ServerXpRank = user.GetServerXpRank(server).Item1;
            GlobalXpRank = user.GetGlobalXpRank().Result.Item1;
            TotalServerXpUsers = user.GetServerXpRank(server).Item2;
            TotalGlobalXpUsers = user.GetGlobalXpRank().Result.Item2;
        }
        public string Username { get; set; }
        public string Discriminator { get; set; }
        public int ServerXp { get; set; }
        public int GlobalXp { get; set; }
        public string ProfileUrl { get; set; }
        public int ServerXpRank { get; set; }
        public int GlobalXpRank { get; set; }
        public int TotalServerXpUsers { get; set; }
        public int TotalGlobalXpUsers { get; set; }
    }
}
