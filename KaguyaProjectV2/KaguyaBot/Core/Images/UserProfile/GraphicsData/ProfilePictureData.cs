using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models;
using SixLabors.ImageSharp.PixelFormats;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.GraphicsData
{
    public static class ProfilePictureData
    {
        public static ProfileTemplateIcon ProfileIcon(SocketGuildUser user)
        {
            var nameFontSize = user.Username.Length < 20 
                ? 40
                : 40 - (user.Username.Length - 25);
            return new ProfileTemplateIcon
            {
                Loc = new ProfileTemplateLoc
                {
                    X = 79,
                    Y = 80
                },
                ProfileUrl = user.GetAvatarUrl(ImageFormat.Png, 137),
                Show = true,
                UsernameText = new ProfileTemplateText
                {
                    Color = Rgba32.Orange,
                    Font = GraphicsConstants.Font(nameFontSize + (20 - user.Username.Length)),
                    Loc = new ProfileTemplateLoc
                    {
                        X = 133,
                        Y = 10
                    },
                    Show = true,
                    Text = user.Username
                },
                UserDiscriminatorText = new ProfileTemplateText
                {
                    Color = Rgba32.DarkOrange,
                    Font = GraphicsConstants.Font(22),
                    Loc = new ProfileTemplateLoc
                    {
                        X = 145,
                        Y = 57
                    },
                    Show = true,
                    Text = $"#{user.Discriminator}"
                }
            };
        }
    }
}
