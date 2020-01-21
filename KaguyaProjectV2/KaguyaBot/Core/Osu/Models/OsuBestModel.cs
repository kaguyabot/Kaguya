using System;

namespace KaguyaProjectV2.KaguyaBot.Core.Osu.Models
{
    public class OsuBestModel : OsuScoreableModel
    {
        public int PlayNumber { get; set; }
        public int BeatmapId { get; set; }
        public int Score { get; set; }
        //public double StarRating { get; set; }
        public int MaxCombo { get; set; }
        //public int Perfect { get; set; }
        public int EnabledMods { get; set; }
        public string StringMods { get; set; }
        //public string UserId { get; set; }
        public string Rank { get; set; }
        public string RankEmote { get; set; }
        public double PP { get; set; }
        public double Accuracy { get; set; }
        public DateTime Date { get; set; }
        public OsuBeatmapModel Beatmap { get; set; }
    }
}
