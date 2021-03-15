using Kaguya.Internal.Services;
using Xunit;

namespace KaguyaTests
{
	public class TestFish
	{
		[Theory]
		[InlineData(1.00, FishRarity.Legendary)]
		[InlineData(0.994, FishRarity.UltraRare)]
		[InlineData(0.97, FishRarity.Rare)]
		[InlineData(0.84, FishRarity.Uncommon)]
		[InlineData(0.54, FishRarity.Common)]
		[InlineData(0.29, FishRarity.Trash)]
		public void TestFishRarityUpperLimits(decimal roll, FishRarity rarity) { Assert.True(FishService.SelectRarity(roll) == rarity); }

		[Theory]
		[InlineData(0.995, FishRarity.Legendary)]
		[InlineData(0.98, FishRarity.UltraRare)]
		[InlineData(0.85, FishRarity.Rare)]
		[InlineData(0.55, FishRarity.Uncommon)]
		[InlineData(0.30, FishRarity.Common)]
		[InlineData(0.00, FishRarity.Trash)]
		public void TestFishRarityLowerLimits(decimal roll, FishRarity rarity) { Assert.True(FishService.SelectRarity(roll) == rarity); }
	}
}