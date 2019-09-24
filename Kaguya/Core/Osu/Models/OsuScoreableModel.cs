using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Core.Osu.Models
{
    public class OsuScoreableModel : OsuBaseModel
    {
        public int count50 { get; set; }
        public int count100 { get; set; }
        public int count300 { get; set; }
        public int countmiss { get; set; }
        public int countkatu { get; set; }
        public int countgeki { get; set; }
    }
}
