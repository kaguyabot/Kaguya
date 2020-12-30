using System;
using System.Reflection;
using Discord.WebSocket;
using Kaguya.Discord;
using Xunit;

namespace KaguyaTests
{
    public class TestWordFilterCaseSensitive
    {
        [Theory]
        [InlineData("the quick brown fox jumped over the lazy dog", "the")]
        [InlineData("the quick brown fox jumped over the lazy dog", "quick")]
        [InlineData("the quick brown fox jumped over the lazy dog", "brown")]
        [InlineData("the quick brown fox jumped over the lazy dog", "fox")]
        [InlineData("the quick brown fox jumped over the lazy dog", "jumped")]
        [InlineData("the quick brown fox jumped over the lazy dog", "over")]
        [InlineData("the quick brown fox jumped over the lazy dog", "lazy")]
        [InlineData("the quick brown fox jumped over the lazy dog", "dog")]
        public void ShouldExactMatch(string content, string pattern)
        {
            Assert.True(DiscordWorker.FilterMatch(content, pattern));
        }

        [Theory]
        [InlineData("the quick brown fox jumped over the lazy dog", "mom")]
        [InlineData("the quick brown fox jumped over the lazy dog", "dad")]
        [InlineData("the quick brown fox jumped over the lazy dog", "discord")]
        [InlineData("the quick brown fox jumped over the lazy dog", "kaguya")]
        [InlineData("the quick brown fox jumped over the lazy dog", "omgwowwtfdoge")]
        [InlineData("the quick brown fox jumped over the lazy dog", "word*with*stars")]
        [InlineData("the quick brown fox jumped over the lazy dog", "ju*ped")]
        [InlineData("the quick brown fox jumped over the lazy dog", "qui*k")]
        [InlineData("the quick brown fox jumped over the lazy dog", "l*z")]
        public void ShouldNotExactMatch(string content, string pattern)
        {
            Assert.False(DiscordWorker.FilterMatch(content, pattern));
        }

        [Theory]
        [InlineData("the quick brown fox jumped over the lazy dog", "*he")]
        [InlineData("the quick brown fox jumped over the lazy dog", "*ick")]
        [InlineData("the quick brown fox jumped over the lazy dog", "*wn")]
        [InlineData("the quick brown fox jumped over the lazy dog", "*ox")]
        [InlineData("the quick brown fox jumped over the lazy dog", "*ped")]
        [InlineData("the quick brown fox jumped over the lazy dog", "*ver")]
        [InlineData("the quick brown fox jumped over the lazy dog", "*zy")]
        [InlineData("the quick brown fox jumped over the lazy dog", "*g")]
        public void ShouldWildCardStartMatch(string content, string pattern)
        {
            Assert.True(DiscordWorker.FilterMatch(content, pattern));
        }
        
        [Theory]
        [InlineData("the quick brown fox jumped over the lazy dog", "*qasdf")]
        [InlineData("the quick brown fox jumped over the lazy dog", "*floop")]
        [InlineData("the quick brown fox jumped over the lazy dog", "*grumbo")]
        [InlineData("the quick brown fox jumped over the lazy dog", "*oxford")]
        [InlineData("the quick brown fox jumped over the lazy dog", "*discord")]
        [InlineData("the quick brown fox jumped over the lazy dog", "*algernon")]
        [InlineData("the quick brown fox jumped over the lazy dog", "*harry")]
        [InlineData("the quick brown fox jumped over the lazy dog", "*hopper")]
        public void ShouldNotWildCardStartMatch(string content, string pattern)
        {
            Assert.False(DiscordWorker.FilterMatch(content, pattern));
        }
        
        [Theory]
        [InlineData("the quick brown fox jumped over the lazy dog", "th*")]
        [InlineData("the quick brown fox jumped over the lazy dog", "qui*")]
        [InlineData("the quick brown fox jumped over the lazy dog", "br*")]
        [InlineData("the quick brown fox jumped over the lazy dog", "f*")]
        [InlineData("the quick brown fox jumped over the lazy dog", "jumpe*")]
        [InlineData("the quick brown fox jumped over the lazy dog", "ov*")]
        [InlineData("the quick brown fox jumped over the lazy dog", "laz*")]
        [InlineData("the quick brown fox jumped over the lazy dog", "d*")]
        public void ShouldWildCardEndMatch(string content, string pattern)
        {
            Assert.True(DiscordWorker.FilterMatch(content, pattern));
        }
        
        [Theory]
        [InlineData("the quick brown fox jumped over the lazy dog", "mork*")]
        [InlineData("the quick brown fox jumped over the lazy dog", "mindi*")]
        [InlineData("the quick brown fox jumped over the lazy dog", "cheers*")]
        [InlineData("the quick brown fox jumped over the lazy dog", "fraiser*")]
        [InlineData("the quick brown fox jumped over the lazy dog", "largehadroncollider*")]
        [InlineData("the quick brown fox jumped over the lazy dog", "murica*")]
        [InlineData("the quick brown fox jumped over the lazy dog", "france*")]
        [InlineData("the quick brown fox jumped over the lazy dog", "canada*")]
        public void ShouldNotWildCardEndMatch(string content, string pattern)
        {
            Assert.False(DiscordWorker.FilterMatch(content, pattern));
        }
    }
}