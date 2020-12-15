using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Kaguya.Services
{
	public class FishService
	{
		private static readonly Random _random = new Random();
		public enum FishRarity
		{
			Legendary,
			UltraRare,
			Rare,
			Common,
			Trash
		}
		public enum FishType
		{
			// TODO: Rarity calculation based on position in this list.
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
			Plastic, // Trash
			Rock,
			Shoe,
			BaitStolen
		}

		public readonly Dictionary<FishType, FishRarity> FishMap = new Dictionary<FishType, FishRarity>
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
		   {FishType.PinFish, FishRarity.Common},
		   {FishType.Bass, FishRarity.Common},
		   {FishType.Catfish, FishRarity.Common},
		   {FishType.Carp, FishRarity.Common},
		   {FishType.Snapper, FishRarity.Common},
		   {FishType.AlgaeEater, FishRarity.Common},
		   {FishType.Anglerfish, FishRarity.Common},
		   {FishType.Jellyfish, FishRarity.Common},
		   {FishType.Frogfish, FishRarity.Common},
		   {FishType.SandShark, FishRarity.Common},
		   {FishType.Dogfish, FishRarity.Common},
		   {FishType.Pigfish, FishRarity.Common},
		   {FishType.Stingray, FishRarity.Common},
		   {FishType.Plastic, FishRarity.Trash},
		   {FishType.Rock, FishRarity.Trash},
		   {FishType.Shoe, FishRarity.Trash},
		   {FishType.BaitStolen, FishRarity.Trash}
		};

		public FishRarity SelectRandomRarity()
		{
			// TODO: Implement.
		}
	}
}