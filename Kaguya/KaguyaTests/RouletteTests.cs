using Kaguya.Discord.Commands.Games;
using Xunit;

namespace KaguyaTests
{
	public class RouletteTests
	{
		[Theory]
		[InlineData("fwefwefew", null, false)]
		[InlineData("straight*", null, false)]
		[InlineData("split*", null, false)]
		[InlineData("triple*", null, false)]
		[InlineData("quad*", null, false)]
		[InlineData("line*", null, false)]
		[InlineData("column*", null, false)]
		[InlineData("dozen*", null, false)]
		[InlineData("low*", null, false)]
		[InlineData("high*", null, false)]
		[InlineData("odd*", null, false)]
		[InlineData("even*", null, false)]
		[InlineData("red*", null, false)]
		[InlineData("black*", null, false)]
		[InlineData("striaght", null, false)]
		[InlineData("straight", new int[] {1}, true)]
		[InlineData("straight", new int[] {1, 2}, false)]
		[InlineData("split", new int[] {1, 2}, true)]
		[InlineData("split", new int[] {1, 2, 3}, false)]
		[InlineData("split", new int[] {1}, false)]
		[InlineData("triple", new int[] {1, 2, 3}, true)]
		[InlineData("triple", new int[] {1, 2, 3, 4}, false)]
		[InlineData("triple", new int[] {1, 2}, false)]
		[InlineData("quad", new int[] {1, 2, 3, 4}, true)]
		[InlineData("quad", new int[] {1}, false)]
		[InlineData("quad", new int[] {1, 2, 3, 4, 5}, false)]
		[InlineData("line", new int[] {1, 2, 3, 4, 5, 6}, true)]
		[InlineData("line", new int[] {1, 2}, false)]
		[InlineData("line", new int[] {1, 2, 3, 4, 5, 6, 7}, false)]
		[InlineData("column", new int[] {1}, true)]
		[InlineData("column", new int[] {2}, true)]
		[InlineData("column", new int[] {3}, true)]
		[InlineData("column", new int[] {1, 2}, false)]
		[InlineData("column", new int[] {2, 3}, false)]
		[InlineData("column", new int[] {3, 2, 1}, false)]
		[InlineData("column", new int[] {4}, false)]
		[InlineData("column", new int[] {0}, false)]
		[InlineData("dozen", new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12}, true)]
		[InlineData("dozen", new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13}, false)]
		[InlineData("dozen", new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11}, false)]
		[InlineData("low", new int[] {1, 2, 3}, false)]
		[InlineData("low", null, true)]
		[InlineData("high", new int[] {1, 2, 3}, false)]
		[InlineData("high", null, true)]
		[InlineData("odd", new int[] {1, 2, 3}, false)]
		[InlineData("odd", null, true)]
		[InlineData("even", new int[] {1, 2, 3}, false)]
		[InlineData("even", null, true)]
		[InlineData("red", new int[] {1, 2, 3}, false)]
		[InlineData("red", null, true)]
		[InlineData("black", new int[] {1, 2, 3}, false)]
		[InlineData("black", null, true)]
		public void TestValidity(string gameArg, int[] nums, bool assertion)
		{
			var parser = new RouletteArgParser(gameArg, nums);
			Assert.True(parser.IsValid() == assertion);
		}
	}
}