using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Modules
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
        public string mods;
        public DateTime date;

        public PlayData(string mapTitle, string mapID, double pp, double difficultyRating, string version, string country, double count300, double count100, double count50, double countMiss, double accuracy, string grade, double playerMaxCombo, double mapMaxCombo, string mods, DateTime date)
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
            this.mods = mods;
            this.date = date;
        }
    }
}
