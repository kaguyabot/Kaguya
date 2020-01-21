using System;

namespace KaguyaProjectV2.KaguyaBot.Core.Osu.Models
{
    public class OsuRecentModel : OsuScoreableModel
    {
        public int BeatmapId { get; set; }
        //public int Score { get; set; }
        public int CountTotal { get; set; }
        public int MaxCombo { get; set; }
        public int EnabledMods { get; set; }
        //public string Perfect { get; set; }
        public string ModString { get; set; }
        //public string UserId { get; set; }
        public string Rank { get; set; }
        public string RankEmote { get; set; }
        public double Accuracy { get; set; }
        public double Completion { get; set; }
        public double PP { get; set; }
        public double FullComboPP { get; set; }
        public DateTime Date { get; set; }
        public OsuBeatmapModel Beatmap { get; set; }
    }
}
