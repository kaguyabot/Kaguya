using Kaguya.Core.Osu.Models;
using System;
using System.Text;

namespace Kaguya.Core.Osu.Builder
{
    public class OsuBestBuilder : OsuBaseBuilder<OsuBestModel>
    {
        public string UserId; // u
        public int Mode; // m
        public int Limit; // limit

        public override string Build(StringBuilder urlBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
