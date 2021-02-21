using Kaguya.Discord.Commands.Administration;
using Xunit;

namespace KaguyaTests
{
    public class TestTextChannelValidation
    {
        [Theory]
        [InlineData("THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG")]
        [InlineData("The quick brown fox jumped over the lazy dog")]
        [InlineData("1")]
        [InlineData("1 _ ")]
        [InlineData("2 ")]
        [InlineData("1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ _")]
        [InlineData("1 2 3 4 5 6 7 8 9 0")]
        public void ShouldValidate(string channelName)
        {
            Assert.True(TextChannel.IsValidTextChannelName(channelName, out var _));
        }

        [Theory]
        [InlineData("!@#$%^&*()+")]
        [InlineData("abc !")]
        [InlineData("abc!")]
        [InlineData("abc1@")]
        [InlineData("#my-channel")]
        [InlineData("#channel")]
        [InlineData("^test")]
        [InlineData("te!@#$%^&*()_+st")]
        [InlineData("")]
        [InlineData("1*2*3*4/5*6-7+8+9")]
        [InlineData("`channel`")]
        [InlineData("t~est")]
        public void ShouldFail(string channelName)
        {
            Assert.False(TextChannel.IsValidTextChannelName(channelName, out var _));
        }
    }
}