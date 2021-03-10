using System;
using System.Collections.Generic;
using System.Linq;

namespace Kaguya.Internal.Services
{
	public enum FishRarity
	{
		Legendary,
		UltraRare,
		Rare,
		Uncommon,
		Common,
		Trash
	}

	public enum FishType
	{
		Megamouth, // Legendary
		BigKahuna,
		GoldenTrout,
		PacificBluefinTuna, // Ultra Rare
		KingSalmon,
		GiantSquid,
		YellowfinTuna,
		BlueMarlin,
		GreatWhiteShark,
		HammerheadShark,
		Blowfish, // Rare
		Piranha,
		StoneCrab,
		Bluefish,
		RedDrum,
		LargeSalmon,
		LargeBass,
		PinFish, // Common
		Bass,
		Catfish,
		Carp,
		Snapper,
		AlgaeEater,
		Anglerfish,
		Jellyfish,
		Frogfish,
		SandShark,
		Dogfish,
		Pigfish,
		Stingray,
		PlasticBottle, // Trash
		Rock,
		Shoe,
		BaitStolen
	}

	public class FishService
	{
		// 110 / 200 chance to lose all coins gambled.
		// 90 / 200 chance to profit.
		// Expected value (using point values below): 74.3
		public static readonly (decimal rangeMin, decimal rangeMax) RangeLegendary = (0.995M, 1M);    // 1 / 200 chance
		public static readonly (decimal rangeMin, decimal rangeMax) RangeUltraRare = (0.98M, 0.995M); // 4 / 200 chance
		public static readonly (decimal rangeMin, decimal rangeMax) RangeRare = (0.85M, 0.98M);       // 26 / 200 chance
		public static readonly (decimal rangeMin, decimal rangeMax) RangeUncommon = (0.55M, 0.85M);   // 60 / 200 chance
		public static readonly (decimal rangeMin, decimal rangeMax) RangeCommon = (0.30M, 0.55M);     // 50 / 200 chance
		public static readonly (decimal rangeMin, decimal rangeMax) RangeTrash = (0.0M, 0.30M);       // 60 / 200 chance
		private static readonly Random _random = new();
		public static readonly Dictionary<FishType, FishRarity> FishMap = new()
		{
			{FishType.Megamouth, FishRarity.Legendary},
			{FishType.BigKahuna, FishRarity.Legendary},
			{FishType.GoldenTrout, FishRarity.Legendary},
			{FishType.PacificBluefinTuna, FishRarity.UltraRare},
			{FishType.KingSalmon, FishRarity.UltraRare},
			{FishType.GiantSquid, FishRarity.UltraRare},
			{FishType.YellowfinTuna, FishRarity.UltraRare},
			{FishType.BlueMarlin, FishRarity.UltraRare},
			{FishType.GreatWhiteShark, FishRarity.UltraRare},
			{FishType.HammerheadShark, FishRarity.UltraRare},
			{FishType.Blowfish, FishRarity.Rare},
			{FishType.Piranha, FishRarity.Rare},
			{FishType.StoneCrab, FishRarity.Rare},
			{FishType.Bluefish, FishRarity.Rare},
			{FishType.RedDrum, FishRarity.Rare},
			{FishType.LargeSalmon, FishRarity.Rare},
			{FishType.LargeBass, FishRarity.Rare},
			{FishType.Frogfish, FishRarity.Uncommon},
			{FishType.SandShark, FishRarity.Uncommon},
			{FishType.Dogfish, FishRarity.Uncommon},
			{FishType.Pigfish, FishRarity.Uncommon},
			{FishType.Stingray, FishRarity.Uncommon},
			{FishType.PinFish, FishRarity.Common},
			{FishType.Bass, FishRarity.Common},
			{FishType.Catfish, FishRarity.Common},
			{FishType.Carp, FishRarity.Common},
			{FishType.Snapper, FishRarity.Common},
			{FishType.AlgaeEater, FishRarity.Common},
			{FishType.Anglerfish, FishRarity.Common},
			{FishType.Jellyfish, FishRarity.Common},
			{FishType.PlasticBottle, FishRarity.Trash},
			{FishType.Rock, FishRarity.Trash},
			{FishType.Shoe, FishRarity.Trash},
			{FishType.BaitStolen, FishRarity.Trash}
		};
		private static KeyValuePair<FishType, FishRarity>[] LegendaryFish =>
			FishMap.Where(x => x.Value == FishRarity.Legendary).ToArray();
		private static KeyValuePair<FishType, FishRarity>[] UltraRareFish =>
			FishMap.Where(x => x.Value == FishRarity.UltraRare).ToArray();
		private static KeyValuePair<FishType, FishRarity>[] RareFish =>
			FishMap.Where(x => x.Value == FishRarity.Rare).ToArray();
		private static KeyValuePair<FishType, FishRarity>[] UncommonFish =>
			FishMap.Where(x => x.Value == FishRarity.Uncommon).ToArray();
		private static KeyValuePair<FishType, FishRarity>[] CommonFish =>
			FishMap.Where(x => x.Value == FishRarity.Common).ToArray();
		private static KeyValuePair<FishType, FishRarity>[] TrashFish =>
			FishMap.Where(x => x.Value == FishRarity.Trash).ToArray();

		public static FishRarity SelectRarity(decimal roll)
		{
			if (IsBetween(roll, RangeLegendary))
			{
				return FishRarity.Legendary;
			}

			if (IsBetween(roll, RangeUltraRare))
			{
				return FishRarity.UltraRare;
			}

			if (IsBetween(roll, RangeRare))
			{
				return FishRarity.Rare;
			}

			if (IsBetween(roll, RangeUncommon))
			{
				return FishRarity.Uncommon;
			}

			if (IsBetween(roll, RangeCommon))
			{
				return FishRarity.Common;
			}

			if (IsBetween(roll, RangeTrash))
			{
				return FishRarity.Trash;
			}

			throw new Exception($"No valid FishRarity could be found for the roll {roll}.");
		}

		public static FishType SelectFish(FishRarity rarity)
		{
			var allFish = rarity switch
			{
				FishRarity.Trash => TrashFish,
				FishRarity.Common => CommonFish,
				FishRarity.Uncommon => UncommonFish,
				FishRarity.Rare => RareFish,
				FishRarity.UltraRare => UltraRareFish,
				FishRarity.Legendary => LegendaryFish,
				_ => CommonFish
			};

			int max = allFish.Length;
			int random;

			lock (_random)
			{
				random = _random.Next(max);
			}

			return allFish[random].Key;
		}

		public static (int fishCoins, int fishExp) GetFishValue(FishRarity rarity)
		{
			// This has been calculated to have an exact expected value of 74.3
			// which is just under 75 (how much it costs for a non-premium user to fish).
			return rarity switch
			{
				FishRarity.Trash => (0, 0),
				FishRarity.Common => (25, 5),
				FishRarity.Uncommon => (85, 20),
				FishRarity.Rare => (135, 45),
				FishRarity.UltraRare => (500, 110),
				FishRarity.Legendary => (3000, 700),
				_ => throw new ArgumentOutOfRangeException(nameof(rarity), rarity, null)
			};
		}

		private static bool IsBetween(decimal num, (decimal min, decimal max) range)
		{
			return range.min <= num && num <= range.max;
		}
	}
}