using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.GraphicsData
{
    public static class GlobalBarData
    {
        public static string FontPath = GraphicsConstants.FONT_PATH;

        public static ProfileTemplateXpBar Bar(User user)
        {
            const float aX = 37f;
            const float aY = 174.5f;
            const float bX = aX;
            const float bY = aY + 26;

            return new ProfileTemplateXpBar
            {
                // What color should fill the bar?
                Color = Rgba32.BlueViolet,
                Length = 277,
                LocA = new ProfileTemplateLoc
                {
                    X = aX,
                    Y = aY
                },
                LocB = new ProfileTemplateLoc
                {
                    X = bX,
                    Y = bY
                },
                TopLeftText = new ProfileTemplateText
                {
                    Color = Rgba32.WhiteSmoke,
                    Loc = new ProfileTemplateLoc
                    {
                        X = 45,
                        Y = 181
                    },
                    Font = GraphicsConstants.Font(16),
                    Show = true,
                    Text = "Global",
                    HasStroke = true,
                    StrokeWidth = 1,
                    StrokeColor = Rgba32.WhiteSmoke
                },
                BottomRightText = new ProfileTemplateText
                {
                    Color = Rgba32.WhiteSmoke,
                    Loc = new ProfileTemplateLoc
                    {
                        X = 199,
                        Y = 181
                    },
                    Font = GraphicsConstants.Font(15),
                    Show = true,
                    Text = $"{user.Experience.ToAbbreviatedForm()} / {user.NextGlobalLevelExp().ToAbbreviatedForm()}",
                    HasStroke = true,
                    StrokeWidth = 1,
                    StrokeColor = Rgba32.WhiteSmoke
                },
                CenterText = new ProfileTemplateText
                {
                    Color = Rgba32.LightSalmon,
                    Loc = new ProfileTemplateLoc
                    {
                        X = 103,
                        Y = 181
                    },
                    Font = GraphicsConstants.Font(16),
                    Show = true,
                    Text = $"Lvl {Math.Floor(user.GlobalLevel())}: {user.PercentToNextLevel() * 100:N0}%",
                    HasStroke = true,
                    StrokeWidth = 1,
                    StrokeColor = Rgba32.WhiteSmoke
                }
            };
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Local
        public static PointF[] GlobalXpBarCoordinates(User user, ProfileTemplateXp xp)
        {
            return new PointF[]
            {
                new PointF(xp.GlobalBar.LocA.X, xp.GlobalBar.LocA.Y), // Base point, top left.
                new PointF(xp.GlobalBar.LocB.X, xp.GlobalBar.LocB.Y), // Base point, bottom left.
                new PointF(GetGlobalXpBarFillCoordinate(user, xp.GlobalBar), xp.GlobalBar.LocB.Y),
                new PointF(GetGlobalXpBarFillCoordinate(user, xp.GlobalBar), xp.GlobalBar.LocA.Y)
            };
        }

        /// <summary>
        /// Returns the x-coordinate for how far we should fill the xp bar based on the user's
        /// required exp to level up.
        /// </summary>
        /// <returns></returns>
        private static float GetGlobalXpBarFillCoordinate(User user, ProfileTemplateXpBar bar)
        {
            var percentToNextLevel = user.PercentToNextLevel();
            return (float)((percentToNextLevel * bar.Length) + bar.LocA.X);
        }
    }
}
