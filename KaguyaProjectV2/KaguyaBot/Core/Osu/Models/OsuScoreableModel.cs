namespace KaguyaProjectV2.KaguyaBot.Core.Osu.Models
{
    public class OsuScoreableModel : OsuBaseModel
    {
        public int Count50 { get; set; }
        public int Count100 { get; set; }
        public int Count300 { get; set; }
        public int Countmiss { get; set; }
        //public int Countkatu { get; set; }
        //public int Countgeki { get; set; }
    }
}
