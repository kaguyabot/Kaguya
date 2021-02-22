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
            Assert.True(ExperienceService.CalculateLevel(exp) - level == 0.0M);
        }

        [Theory]
        [InlineData(257586, 179.41641508)]
        public void FailLevelCalculation(int exp, decimal level)
        {
            Assert.False(ExperienceService.CalculateLevel(exp) - level == 0.0M);
        }

        [Theory]
        [InlineData(1.54, 54)]
        [InlineData(1000.54, 54)]
        [InlineData(0.01, 1)]
        [InlineData(100.999, 99.9)]
        [InlineData(1.5, 50)]
        public void TestPercentToNextLevelCalculation(decimal level, decimal check)
        {
            decimal progress = ExperienceService.CalculatePercentToNextLevel(level);
            _testOutputHelper.WriteLine($"Level: {level} | Progress: {progress}%");
            Assert.True(progress == check);
        }
    }
}