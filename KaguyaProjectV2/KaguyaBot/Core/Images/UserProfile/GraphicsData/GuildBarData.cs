using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.GraphicsData
{
    public static class GuildBarData
    {
        public static ProfileTemplateXpBar Bar(User user, Server server)
        {
            return new ProfileTemplateXpBar
            {
                Color = Rgba32.BlueViolet,
                Length = 446,
                LocA = new ProfileTemplateLoc
                {
                    X = 38.5f,
                    Y = 213f
                },
                LocB = new ProfileTemplateLoc
                {
                    X = 37.5f,
                    Y = 253.5f
                },
                TopLeftText = new ProfileTemplateText
                {
                    Color = Rgba32.LightCoral,
                    Loc = new ProfileTemplateLoc
                    {
                        X = 44,
                        Y = 216
                    },
                    Font = GraphicsConstants.Font(18),
                    HasStroke = true,
                    Show = true,
                    StrokeColor = Rgba32.WhiteSmoke,
                    StrokeWidth = 1,
                    Text = "Server Level"
                },
                BottomRightText = new ProfileTemplateText
                {
                    Color = Rgba32.LightCoral,
                    Loc = new ProfileTemplateLoc
                    {
                        X = 345,
                        Y = 230
                    },
                    Font = GraphicsConstants.Font(22),
                    HasStroke = true,
                    Show = true,
                    StrokeColor = Rgba32.WhiteSmoke,
                    StrokeWidth = 1,
                    Text = $"{user.ServerExp(server).ToAbbreviatedForm()} / {user.NextServerLevelExp(server).ToAbbreviatedForm()}"
                },
                CenterText = new ProfileTemplateText
                {
                    Color = Rgba32.LightCoral,
                    Loc = new ProfileTemplateLoc
                    {
                        X = 185,
                        Y = 224
                    },
                    Font = GraphicsConstants.Font(22),
                    HasStroke = true,
                    Show = true,
                    StrokeColor = Color.WhiteSmoke,
                    StrokeWidth = 1,
                    Text = $"Lvl {Math.Floor(user.ServerLevel(server))}: {user.PercentToNextServerLevel(server) * 100:N1}%"
                }
            };
        }

        public static PointF[] GuildXpBarCoordinates(User user, Server server, ProfileTemplateXp xp)
        {
            return new PointF[]
            {
                new PointF(xp.GuildBar.LocA.X, xp.GuildBar.LocA.Y), // Base point, top left.
                //new PointF(xp.GuildBar.LocB.X, xp.GuildBar.LocB.Y), // Base point, bottom left.
                new PointF(xp.GuildBar.LocB.X, xp.GuildBar.LocB.Y - 9), // Base point, bottom left.
                new PointF(xp.GuildBar.LocB.X + 12, xp.GuildBar.LocB.Y), // Base point, bottom left.
                new PointF(GetGuildXpBarFillCoordinate(user, server, xp.GuildBar), xp.GuildBar.LocB.Y),
                new PointF(GetGuildXpBarFillCoordinate(user, server, xp.GuildBar), xp.GuildBar.LocA.Y)
            };
        }

        /// <summary>
        /// Returns the x-coordinate for how far we should fill the xp bar based on the user's
        /// required exp to level up.
        /// </summary>
        /// <returns></returns>
        private static float GetGuildXpBarFillCoordinate(User user, Server server, ProfileTemplateXpBar bar)
        {
            var percentToNextLevel = user.PercentToNextServerLevel(server);
            if(percentToNextLevel < 1)
                return (float)((percentToNextLevel * bar.Length) + bar.LocA.X) + 5;
            return (float)((percentToNextLevel * bar.Length) + bar.LocA.X);
        }
    }
}
