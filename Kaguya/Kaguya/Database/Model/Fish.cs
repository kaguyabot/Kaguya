using System;
using Humanizer;
using Kaguya.Services;

namespace Kaguya.Database.Model
{ 
	public class Fish
    {
        public long FishId { get; init; }
        public ulong UserId { get; init; }
        public ulong ServerId { get; init; }
        public DateTime TimeCaught { get; set; }
        public int ExpValue { get; set; }
        public int PointValue { get; set; }
        public FishService.FishType FishType { get; set; }
        public string FishTypeString => FishType.Humanize(LetterCasing.Title);
        public bool Sold { get; set; }
    }
}