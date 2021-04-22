using Kaguya.Discord.Commands.Games;
using Kaguya.Internal.Enums;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace KaguyaTests
{
	public class TestDice
	{
		[Fact]
		private void TestDiceRoll()
		{
			var rolls = new List<int>();
			for (int i = 0; i < 100000; i++)
			{
				rolls.Add(Dice.GenerateDiceRoll());
			}
			
			Assert.DoesNotContain(rolls, x => x is < 1 or > 6);
		}

		[Theory]
		[InlineData(2, DicePrediction.Lower)]
		[InlineData(3, DicePrediction.Lower)]
		[InlineData(4, DicePrediction.Lower)]
		[InlineData(5, DicePrediction.Lower)]
		[InlineData(6, DicePrediction.Lower)]
		[InlineData(7, DicePrediction.Exact)]
		[InlineData(8, DicePrediction.Higher)]
		[InlineData(9, DicePrediction.Higher)]
		[InlineData(10, DicePrediction.Higher)]
		[InlineData(11, DicePrediction.Higher)]
		[InlineData(12, DicePrediction.Higher)]
		private void TestDiceValidPrediction(int roll, DicePrediction prediction)
		{
			Assert.True(Dice.IsCorrectGuess(roll, prediction));
		}
		
		[Theory]
		[InlineData(12, DicePrediction.Lower)]
		[InlineData(11, DicePrediction.Lower)]
		[InlineData(10, DicePrediction.Lower)]
		[InlineData(9, DicePrediction.Lower)]
		[InlineData(8, DicePrediction.Lower)]
		[InlineData(7, DicePrediction.Lower)]
		[InlineData(7, DicePrediction.Higher)]
		[InlineData(6, DicePrediction.Higher)]
		[InlineData(5, DicePrediction.Higher)]
		[InlineData(4, DicePrediction.Higher)]
		[InlineData(3, DicePrediction.Higher)]
		[InlineData(2, DicePrediction.Higher)]
		[InlineData(12, DicePrediction.Exact)]
		[InlineData(11, DicePrediction.Exact)]
		[InlineData(10, DicePrediction.Exact)]
		[InlineData(9, DicePrediction.Exact)]
		[InlineData(8, DicePrediction.Exact)]
		[InlineData(6, DicePrediction.Exact)]
		[InlineData(5, DicePrediction.Exact)]
		[InlineData(4, DicePrediction.Exact)]
		[InlineData(3, DicePrediction.Exact)]
		[InlineData(2, DicePrediction.Exact)]
		private void TestDiceInValidPrediction(int roll, DicePrediction prediction)
		{
			Assert.False(Dice.IsCorrectGuess(roll, prediction));
		}

		[Theory]
		[InlineData(2, DicePrediction.Lower)]
		[InlineData(3, DicePrediction.Lower)]
		[InlineData(4, DicePrediction.Lower)]
		[InlineData(5, DicePrediction.Lower)]
		[InlineData(6, DicePrediction.Lower)]
		[InlineData(7, DicePrediction.Exact)]
		[InlineData(8, DicePrediction.Higher)]
		[InlineData(9, DicePrediction.Higher)]
		[InlineData(10, DicePrediction.Higher)]
		[InlineData(11, DicePrediction.Higher)]
		[InlineData(12, DicePrediction.Higher)]
		private void TestPayoutSuccess(int roll, DicePrediction prediction)
		{
			if (!Dice.IsCorrectGuess(roll, prediction))
			{
				throw new XunitException("Prediction was invalid. Check inline data.");
			}
			
			const int bet = 1000;
			int payout = Dice.GetPayout(bet, roll, prediction);
			
			if (prediction == DicePrediction.Exact)
			{
				Assert.True(payout == (bet * Dice.ExactPayoutMultiplier));
			}
			else
			{
				Assert.True(payout == (bet * Dice.HLPayoutMultiplier));
			}
		}
	}
}