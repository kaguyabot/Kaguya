using System;
using Kaguya.Internal.Services;
using Xunit;

namespace KaguyaTests
{
    public class TextExperienceService
    {
        // Math.Sqrt((exp / 8) - 7) is the formula the service is tested against for exp.
        [Theory]
        [InlineData(64, 1)]
        [InlineData(257586, 179.419201871)]
        public void TestLevelCalculation(int exp, double level)
        {
            double difference = Math.Abs(level * .00001);
            Assert.True(Math.Abs(ExperienceService.CalculateLevel(exp) - level) <= difference);
        }

        [Theory]
        [InlineData(257586, 179.41641508)]
        public void FailLevelCalculation(int exp, double level)
        {
            double difference = Math.Abs(level * .00001);
            Assert.False(Math.Abs(ExperienceService.CalculateLevel(exp) - level) <= difference);
        }
    }
}