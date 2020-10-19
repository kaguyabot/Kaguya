using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Images.Models;
using LinqToDB.Common;
using SixLabors.ImageSharp.PixelFormats;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models
{
    public static class ProfilePictureData
    {
        public static TemplateIcon ProfileIcon(SocketGuildUser user)
        {
            string name = user.Username.IsNullOrEmpty() ? $"User Id: {user.Id}" : user.Username;
            string writtenUsername = "";

            foreach (char c in name)
            {
                if (!c.ToString().ContainsEmoji())
                    writtenUsername += c;
            }

            if (string.IsNullOrWhiteSpace(writtenUsername))
                writtenUsername = $"User Id: {user.Id}";

            return new TemplateIcon
            {
                Loc = new TemplateLoc
                {
                    X = 79,
                    Y = 80
                },
                ProfileUrl = user.GetAvatarUrl(ImageFormat.Png, 137),
                Show = true,
                UsernameText = new TemplateText
                {
                    Color = Rgba32.Orange,
                    Font = ImageBase.Font(user.Username, 40, 20),
                    Loc = new TemplateLoc
                    {
                        X = 133,
                        Y = 10
                    },
                    Show = true,
                    Text = writtenUsername,
                    HasStroke = true,
                    StrokeWidth = 2
                },
                UserDiscriminatorText = new TemplateText
                {
                    Color = Rgba32.DarkOrange,
                    Font = ImageBase.Font(22),
                    Loc = new TemplateLoc
                    {
                        X = 145,
                        Y = 57
                    },
                    Show = true,
                    Text = $"#{user.Discriminator}",
                    HasStroke = true,
                    StrokeWidth = 0.5f
                }
            };
        }
    }
}