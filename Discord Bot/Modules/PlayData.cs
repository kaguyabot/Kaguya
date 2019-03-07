using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Modules
{
    public class PlayData
    {
        public string mapTitle;
        public string mapID;
        public double pp;
        public double difficultyRating;
        public string version;
        public double count300;
        public double count100;
        public double count50;
        public double countMiss;
        public double accuracy;
        public string grade;
        public double playerMaxCombo;
        public double mapMaxCombo;
        

        public PlayData(string mapTitle, string mapID, double pp, double difficultyRating, string version, string country, double count300, double count100, double count50, double countMiss, double accuracy, string grade, double playerMaxCombo, double mapMaxCombo)
        {
            this.mapTitle = mapTitle;
            this.mapID = mapID;
            this.pp = pp;
            this.difficultyRating = difficultyRating;
            this.version = version;
            this.count300 = count300;
            this.count100 = count100;
            this.count50 = count50;
            this.countMiss = countMiss;
            this.accuracy = accuracy;
            this.grade = grade;
            this.playerMaxCombo = playerMaxCombo;
            this.mapMaxCombo = mapMaxCombo;
        }
    }

    public class Player
    {
        public long UserId { get; set; }
        public string Username { get; set; }
        public DateTimeOffset JoinDate { get; set; }
        public long Count300 { get; set; }
        public long Count100 { get; set; }
        public long Count50 { get; set; }
        public long Playcount { get; set; }
        public string RankedScore { get; set; }
        public string TotalScore { get; set; }
        public long PPRank { get; set; }
        public string Level { get; set; }
        public string PPRaw { get; set; }
        public string Accuracy { get; set; }
        public long CountRankSS { get; set; }
        public long CountRankSSH { get; set; }
        public long CountRankS { get; set; }
        public long CountRankSH { get; set; }
        public long CountRankA { get; set; }
        public string Country { get; set; }
        public long TotalSecondsPlayed { get; set; }
        public long PpCountryRank { get; set; }
        public string Events { get; set; }
    }

    public class PlayerBest
    {
        public long BeatmapId { get; set; }
        public long Score { get; set; }
        public long Maxcombo { get; set; }
        public long Count50 { get; set; }
        public long Count100 { get; set; }
        public long Count300 { get; set; }
        public long Countmiss { get; set; }
        public long Countkatu { get; set; }
        public long Countgeki { get; set; }
        public long Perfect { get; set; }
        public long EnabledMods { get; set; }
        public long UserId { get; set; }
        public DateTimeOffset Date { get; set; }
        public string PP { get; set; }
    }

    public class RecentScore
    {
        public long BeatmapId { get; set; }
        public long Score { get; set; }
        public long Maxcombo { get; set; }
        public long Count50 { get; set; }
        public long Count100 { get; set; }
        public long Count300 { get; set; }
        public long Countmiss { get; set; }
        public long Countkatu { get; set; }
        public long Countgeki { get; set; }
        public long Perfect { get; set; }
        public long EnabledMods { get; set; }
        public long UserId { get; set; }
        public DateTimeOffset Date { get; set; }
    }

    public class Beatmap
    {
        public long BeatmapsetId { get; set; }
        public long BeatmapId { get; set; }
        public long Approved { get; set; }
        public long TotalLength { get; set; }
        public long HitLength { get; set; }
        public string Version { get; set; }
        public string FileMd5 { get; set; }
        public string DiffSize { get; set; }
        public string DiffOverall { get; set; }
        public string DiffApproach { get; set; }
        public long DiffDrain { get; set; }
        public long Mode { get; set; }
        public object ApprovedDate { get; set; }
        public DateTimeOffset LastUpdate { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Creator { get; set; }
        public long CreatorId { get; set; }
        public long Bpm { get; set; }
        public string Source { get; set; }
        public string Tags { get; set; }
        public long GenreId { get; set; }
        public long LanguageId { get; set; }
        public long FavouriteCount { get; set; }
        public long Playcount { get; set; }
        public long Passcount { get; set; }
        public long MaxCombo { get; set; }
        public double Difficultyrating { get; set; }
    }
}
