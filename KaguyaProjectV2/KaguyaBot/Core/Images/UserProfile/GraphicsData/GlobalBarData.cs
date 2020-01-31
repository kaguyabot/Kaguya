using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
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
            const float length = 277;
            const float aX = 37f;
            const float aY = 174.5f;
            const float bX = aX;
            const float bY = aY + 26;

            return new ProfileTemplateXpBar
            {
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
                    Color = Rgba32.LightCoral,
                    Loc = new ProfileTemplateLoc
                    {
                        X = aX + 8,
                        Y = 181
                    },
                    Font = GraphicsConstants.Font(16),
                    Show = true,
                    Text = "Global"
                },
                BottomRightText = new ProfileTemplateText
                {
                    Color = Rgba32.LightCoral,
                    Loc = new ProfileTemplateLoc
                    {
                        X = aX + length - 115,
                        Y = 181
                    },
                    Font = GraphicsConstants.Font(15),
                    Show = true,
                    Text = $"{user.Experience.ToAbbreviatedForm()} / {user.NextLevelExp().ToAbbreviatedForm()}"
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
                    Text = $"Lvl {Math.Floor(user.GlobalLevel())}: {GetPercentToNextLevel(user) * 100:N1}%"
                }
            };
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Local
        public static PointF[] GlobalXpBarCoordinates(User user, ProfileTemplateXp xp, ProfileTemplateUserData data)
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
        /// Calculates the difference of EXP between the user's current level (rounded
        /// down to the nearest integer) and one level above this value.
        /// </summary>
        /// <returns></returns>
        private static int XpDifferenceBetweenLevels(User user)
        {
            var curLevel = Math.Floor(user.GlobalLevel());
            var nextLevel = Math.Floor(user.GlobalLevel() + 1);
            var curLevelExp = GlobalProperties.CalculateExpFromLevel(curLevel);
            var nextLevelExp = GlobalProperties.CalculateExpFromLevel(nextLevel);

            return nextLevelExp - curLevelExp;
        }

        /// <summary>
        /// Returns the percentage the user is to reaching their next level.
        /// </summary>
        /// <returns></returns>
        private static double GetPercentToNextLevel(User user)
        {
            var curLevel = user.GlobalLevel();
            var curLevelExpRoundedDown = GlobalProperties.CalculateExpFromLevel(Math.Floor(curLevel));
            var minMaxDifference = XpDifferenceBetweenLevels(user);

            return (((double)user.Experience - curLevelExpRoundedDown) / minMaxDifference);
        }

        /// <summary>
        /// Returns the x-coordinate for how far we should fill the xp bar based on the user's
        /// required exp to level up.
        /// </summary>
        /// <returns></returns>
        private static float GetGlobalXpBarFillCoordinate(User user, ProfileTemplateXpBar bar)
        {
            var percentToNextLevel = GetPercentToNextLevel(user);
            return (float)((percentToNextLevel * bar.Length) + bar.LocA.X);
        }
    }
}
