using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Images.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System;

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models
{
    public static class GlobalBarData
    {
        public static string FontPath = ImageBase.FontPath;

        public static TemplateXpBar Bar(User user)
        {
            const float A_X = 37f;
            const float A_Y = 174.5f;
            const float B_X = A_X;
            const float B_Y = A_Y + 26;

            return new TemplateXpBar
            {
                // What color should fill the bar?
                Color = Rgba32.BlueViolet,
                Length = 277,
                LocA = new TemplateLoc
                {
                    X = A_X,
                    Y = A_Y
                },
                LocB = new TemplateLoc
                {
                    X = B_X,
                    Y = B_Y
                },
                TopLeftText = new TemplateText
                {
                    Color = Rgba32.WhiteSmoke,
                    Loc = new TemplateLoc
                    {
                        X = 45,
                        Y = 181
                    },
                    Font = ImageBase.Font(16),
                    Show = true,
                    Text = "Global",
                    HasStroke = true,
                    StrokeWidth = 1,
                    StrokeColor = Rgba32.WhiteSmoke
                },
                BottomRightText = new TemplateText
                {
                    Color = Rgba32.WhiteSmoke,
                    Loc = new TemplateLoc
                    {
                        X = 199,
                        Y = 181
                    },
                    Font = ImageBase.Font(15),
                    Show = true,
                    Text = $"{user.Experience.ToAbbreviatedForm()} / {user.NextGlobalLevelExp().ToAbbreviatedForm()}",
                    HasStroke = true,
                    StrokeWidth = 1,
                    StrokeColor = Rgba32.WhiteSmoke
                },
                CenterText = new TemplateText
                {
                    Color = Rgba32.LightSalmon,
                    Loc = new TemplateLoc
                    {
                        X = 103,
                        Y = 181
                    },
                    Font = ImageBase.Font(16),
                    Show = true,
                    Text = $"Lvl {Math.Floor(user.GlobalLevel())}: {user.PercentToNextLevel() * 100:N0}%",
                    HasStroke = true,
                    StrokeWidth = 1,
                    StrokeColor = Rgba32.WhiteSmoke
                }
            };
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Local
        public static PointF[] GlobalXpBarCoordinates(User user, ProfileTemplateXp xp) => new PointF[]
        {
            new PointF(xp.GlobalBar.LocA.X, xp.GlobalBar.LocA.Y), // Base point, top left.
            new PointF(xp.GlobalBar.LocB.X, xp.GlobalBar.LocB.Y), // Base point, bottom left.
            new PointF(GetGlobalXpBarFillCoordinate(user, xp.GlobalBar), xp.GlobalBar.LocB.Y),
            new PointF(GetGlobalXpBarFillCoordinate(user, xp.GlobalBar), xp.GlobalBar.LocA.Y)
        };

        /// <summary>
        /// Returns the x-coordinate for how far we should fill the xp bar based on the user's
        /// required exp to level up.
        /// </summary>
        /// <returns></returns>
        private static float GetGlobalXpBarFillCoordinate(User user, TemplateXpBar bar)
        {
            double percentToNextLevel = user.PercentToNextLevel();

            return (float) ((percentToNextLevel * bar.Length) + bar.LocA.X);
        }
    }
}