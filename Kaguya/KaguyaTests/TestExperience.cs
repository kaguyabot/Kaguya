using System;
using Kaguya.Internal.Services;
using Xunit;
using Xunit.Abstractions;

namespace KaguyaTests
{
    public class TextExperienceService
    {
        private readonly ITestOutputHelper _testOutputHelper;
        public TextExperienceService(ITestOutputHelper testOutputHelper) { _testOutputHelper = testOutputHelper; }

        // Math.Sqrt((exp / 8) - 7) is the formula the service is tested against for exp.
        [Theory]
        [InlineData(64, 1)]
        [InlineData(257586, 179.419201871)]
        public void TestLevelCalculation(int exp, decimal level)
        {
            var difference = ExperienceService.CalculateLevel(exp) - level;
            _testOutputHelper.WriteLine($"Exp: {exp} | Level: {level} | Difference: {difference}");
            Assert.True(difference <= 0.001M);
        }

        [Theory]
        [InlineData(257586, 179.41641508)]
        public void FailLevelCalculation(int exp, decimal level)
        {
            Assert.False(ExperienceService.CalculateLevel(exp) - level <= 0.001M);
        }

        [Theory]
        [InlineData(1.54, 54)]
        [InlineData(1000.54, 54)]
        [InlineData(0.01, 1)]
        [InlineData(100.999, 99.9)]
        [InlineData(1.5, 50)]
        [InlineData(1.00, 0.00)]
        [InlineData(1.9999999999999, 99.99999999999)]
        public void TestPercentToNextLevelCalculation(decimal level, decimal check)
        {
            decimal progress = ExperienceService.CalculatePercentToNextLevel(level);
            _testOutputHelper.WriteLine($"Level: {level} | Progress: {progress}%");
            Assert.True(progress == check);
        }

        [Theory]
        [InlineData(2.999, 3.00)]
        [InlineData(1, 2)]
        [InlineData(0, 1)]
        [InlineData(1.1, 2.99)]
        [InlineData(100, 101)]
        [InlineData(0, 999.99)]
        [InlineData(10.24, 11.23)]
        [InlineData(8.88, 9.02)]
        public void TestHasLeveledUpTrue(decimal oldLevel, decimal newLevel)
        {
            Assert.True(ExperienceService.HasLeveledUp(oldLevel, newLevel));
        }

        [Theory]
        [InlineData(9.0, 8.0)]
        [InlineData(0.0, 0.05)]
        [InlineData(0.0, 0.5)]
        [InlineData(0.0, 0.9999)]
        [InlineData(0.0, 0.99999999)]
        [InlineData(1.0, 0.99999999)]
        [InlineData(12.24, 11.25)]
        [InlineData(12.24, 11.23)]
        [InlineData(99.999, 98.999)]
        public void TestHasLeveledUpFalse(decimal oldLevel, decimal newLevel)
        {
            Assert.False(ExperienceService.HasLeveledUp(oldLevel, newLevel));
        }
    }
}