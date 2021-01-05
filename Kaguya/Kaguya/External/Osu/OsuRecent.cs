using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Kaguya.Discord;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Exceptions;
using OsuSharp;
using OsuSharp.Oppai;

namespace Kaguya.External.Osu
{
    public class OsuRecent : OsuBase
    {
        private readonly OsuData _osuData;

        private const char POINT_RIGHT = '▸';

        public OsuRecent(OsuData osuData) : base(osuData.Client)
        {
            _osuData = osuData;
        }

        public async Task<Embed> GetMostRecentForUserAsync()
        {
            var osuUser = await _osuData.GetOsuUserAsync();
            IReadOnlyList<Score> userRecentPlays = await _osuData.Client.GetUserRecentsByUserIdAsync(osuUser.UserId, _osuData.GameMode);

            if (userRecentPlays.Count == 0)
            {
                return new KaguyaEmbedBuilder(KaguyaColors.Red)
                       .WithDescription("The user has no recent plays.")
                       .Build();
            }
            
            Score recentPlay = userRecentPlays[0];

            // Assuming userRecentPlays is already ordered by date.
            int consecutiveTries = 0;
            foreach (var play in userRecentPlays)
            {
                if (play.BeatmapId == recentPlay.BeatmapId)
                {
                    consecutiveTries++;
                }
                else
                {
                    break;
                }
            }
            if (recentPlay == null)
            {
                throw new OsuException("The user has no recent plays.");
            }

            bool calculablePerformance = _osuData.GameMode == GameMode.Standard || _osuData.GameMode == GameMode.Taiko;

            Beatmap recentMapInfo = await recentPlay.GetBeatmapAsync();
            PerformanceData recentPpInfo = await recentPlay.GetPPAsync();
            PerformanceData[] ppVariants =
            {
                await recentMapInfo.GetPPAsync(recentPlay.Mods, 95.0f),
                await recentMapInfo.GetPPAsync(recentPlay.Mods, 98.0f),
                await recentMapInfo.GetPPAsync(recentPlay.Mods, 99.0f),
                await recentMapInfo.GetPPAsync(recentPlay.Mods, 100.0f)
            };
            
            string modString = GetModString(recentPlay.Mods);
            
            // Title
            string title = $"{recentMapInfo.Title}";
            
            // Description
            StringBuilder descSb = new StringBuilder();
            
            // Note: A line should never start or end with a {POINT_RIGHT} character. That is applied after string construction.
            string gradeEmote = GetEmoteForRank(recentPlay.Rank);
            
            string listingUrl = "Listing".AsUrl(recentMapInfo.BeatmapUri.AbsoluteUri);
            string downloadUrl = "Download".AsUrl("https://osu.ppy.sh/beatmapsets/" + recentMapInfo.BeatmapsetId + "/download");
            string line1 = "Map Info:".AsBold() + " " + listingUrl + " " + POINT_RIGHT + downloadUrl;

            string comboInformation = "Combo:".AsBold() + $" x{recentPlay.MaxCombo:N0}/{recentMapInfo.MaxCombo?.ToString("N0") ?? "???"}";
            string scoreInformation = "Score:".AsBold() + $" {recentPlay.TotalScore:N0}";
            string line2 = $"{gradeEmote} {POINT_RIGHT} {comboInformation} {POINT_RIGHT} {scoreInformation}";

            string modInformation = "Mods:".AsBold() + $" {modString}";
            string starInformation = "Stars:".AsBold() + $" {(recentPpInfo.Stars == 0 ? recentMapInfo.StarRating.Value.ToString("N2") : recentPpInfo.Stars.ToString("N2"))}★";
            string line3 = $"{modInformation} {POINT_RIGHT} {starInformation}";

            string difficultyInformation = "Difficulty:".AsBold() + $" [{recentMapInfo.Difficulty}]";
            string line4 = difficultyInformation;
            
            string accInformation = "Accuracy:".AsBold() + $" {recentPpInfo.Accuracy:N2}%";
            string hitCountInformation = string.Empty;

        #region Hit Count Information
            bool has100 = recentPlay.Count100 > 0;
            bool has50 = recentPlay.Count50 > 0;
            bool hasMisses = recentPlay.Miss > 0;

            if (has100 || has50 || hasMisses)
            {
                var missDisplay = new List<string>();
                if (has100)
                {
                    missDisplay.Add($"{recentPlay.Count100:N0}x 100" + (recentPlay.Count100 > 1 ? "s" : ""));
                }

                if (has50)
                {
                    missDisplay.Add($"{recentPlay.Count50:N0}x 50" + (recentPlay.Count50 > 1 ? "s" : ""));
                }

                if (hasMisses)
                {
                    missDisplay.Add($"{recentPlay.Miss:N0} miss" + (recentPlay.Miss > 1 ? "es" : ""));
                }

                string final = string.Empty;
                foreach (var element in missDisplay)
                {
                    final += element + ", ";
                }

                hitCountInformation = $"[{final[..^2]}]";
            }
        #endregion
            
            string line5 = $"{accInformation} {POINT_RIGHT} {hitCountInformation}";

            int calculatedBpm = (recentPpInfo.Mods & Mode.DoubleTime) != 0 || (recentPpInfo.Mods & Mode.Nightcore) != 0
                ? (int)(recentMapInfo.Bpm.GetValueOrDefault() * 1.5)
                : (int)recentMapInfo.Bpm.GetValueOrDefault();
            
            string bpmInformation = "BPM:".AsBold() + $" {calculatedBpm}";
            string drainInformation = "Drain:".AsBold() + $" {recentMapInfo.HitLength:m\\:ss}";
            string csInformation = "CS:".AsBold() + $" {recentPpInfo.Cs:N1}";
            string arInformation = "AR:".AsBold() + $" {recentPpInfo.Ar:N1}";
            string odInformation = "OD:".AsBold() + $" {recentPpInfo.Od:N1}";
            string hpInformation = "HP:".AsBold() + $" {recentPpInfo.Hp:N1}";
            string mapPropertyInformation = $"{csInformation} {arInformation} {odInformation} {hpInformation}";

            bool canDisplayCircleData = calculablePerformance;
            
            string line6 = $"{bpmInformation} {POINT_RIGHT} {drainInformation}" + (canDisplayCircleData ? $" {POINT_RIGHT} [{mapPropertyInformation}]" : "");

            string ppInformation = "PP:".AsBold() + $" {recentPpInfo.Pp:N2}pp";
            string variantPpLine = $"95%: {ppVariants[0].Pp:N0}pp | 98%: {ppVariants[1].Pp:N0}pp | 99%: {ppVariants[2].Pp:N0}pp | 100%: {ppVariants[3].Pp:N0}pp";
            
            string line7 = $"{ppInformation}\n{POINT_RIGHT} {variantPpLine}";
            
            descSb.AppendLine(line1);
            descSb.AppendLine();
            descSb.AppendLine(line2);
            descSb.AppendLine();
            descSb.AppendLine($" {POINT_RIGHT} " + line3);
            descSb.AppendLine($" {POINT_RIGHT} " + line4);
            descSb.AppendLine();
            descSb.AppendLine($" {POINT_RIGHT} " + line5);
            descSb.AppendLine($" {POINT_RIGHT} " + line6);
            
            if (calculablePerformance)
            {
                descSb.AppendLine($" {POINT_RIGHT} " + line7);
            }
            
            // Footer
            string humanizedTimeAgo = !recentPlay.Date.HasValue
                ? string.Empty
                : $" | Submitted " + GetScoreTimespan(recentPlay).HumanizeTraditionalReadable() + " ago.";

            var footer = new EmbedFooterBuilder
            {
                Text = $"Try #{consecutiveTries} - osu!Bancho{humanizedTimeAgo}"
            };

            string playerProfilePicture = "https://a.ppy.sh/" + osuUser.UserId;
            
            return new KaguyaEmbedBuilder(KaguyaColors.Teal)
                   .WithAuthor(osuUser.Username, playerProfilePicture, osuUser.ProfileUri.AbsoluteUri)
                   .WithThumbnailUrl(recentMapInfo.ThumbnailUri.AbsoluteUri)
                   .WithTitle(title)
                   .WithUrl(recentMapInfo.BeatmapUri.AbsoluteUri)
                   .WithDescription(descSb.ToString())
                   .WithFooter(footer)
                   .Build();
        }
    }
}