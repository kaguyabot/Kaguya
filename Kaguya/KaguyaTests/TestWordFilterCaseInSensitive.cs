using Kaguya.Discord;
using Xunit;

namespace KaguyaTests
{
	public class TestWordFilterCaseInSensitive
	{
		[Theory]
		[InlineData("the quick brown fox jumped over the lazy dog", "THE")]
		[InlineData("the quick brown fox jumped over the lazy dog", "QUICK")]
		[InlineData("the quick brown fox jumped over the lazy dog", "BROWN")]
		[InlineData("the quick brown fox jumped over the lazy dog", "FOX")]
		[InlineData("the quick brown fox jumped over the lazy dog", "JUMPED")]
		[InlineData("the quick brown fox jumped over the lazy dog", "OVER")]
		[InlineData("the quick brown fox jumped over the lazy dog", "LAZY")]
		[InlineData("the quick brown fox jumped over the lazy dog", "DOG")]
		public void ShouldExactMatch(string content, string pattern)
		{
			Assert.True(DiscordWorker.FilterMatch(content, pattern));
		}

		[Theory]
		[InlineData("the quick brown fox jumped over the lazy dog", "MOM")]
		[InlineData("the quick brown fox jumped over the lazy dog", "DAD")]
		[InlineData("the quick brown fox jumped over the lazy dog", "DISCORD")]
		[InlineData("the quick brown fox jumped over the lazy dog", "KAGUYA")]
		[InlineData("the quick brown fox jumped over the lazy dog", "OMGWOWWTFDOGE")]
		[InlineData("the quick brown fox jumped over the lazy dog", "WORD*WITH*STARS")]
		[InlineData("the quick brown fox jumped over the lazy dog", "JU*PED")]
		[InlineData("the quick brown fox jumped over the lazy dog", "QUI*K")]
		[InlineData("the quick brown fox jumped over the lazy dog", "L*Z")]
		public void ShouldNotExactMatch(string content, string pattern)
		{
			Assert.False(DiscordWorker.FilterMatch(content, pattern));
		}

		[Theory]
		[InlineData("the quick brown fox jumped over the lazy dog", "*HE")]
		[InlineData("the quick brown fox jumped over the lazy dog", "*ICK")]
		[InlineData("the quick brown fox jumped over the lazy dog", "*WN")]
		[InlineData("the quick brown fox jumped over the lazy dog", "*OX")]
		[InlineData("the quick brown fox jumped over the lazy dog", "*PED")]
		[InlineData("the quick brown fox jumped over the lazy dog", "*VER")]
		[InlineData("the quick brown fox jumped over the lazy dog", "*ZY")]
		[InlineData("the quick brown fox jumped over the lazy dog", "*G")]
		public void ShouldWildCardStartMatch(string content, string pattern)
		{
			Assert.True(DiscordWorker.FilterMatch(content, pattern));
		}
        
		[Theory]
		[InlineData("the quick brown fox jumped over the lazy dog", "*QASDF")]
		[InlineData("the quick brown fox jumped over the lazy dog", "*FLOOP")]
		[InlineData("the quick brown fox jumped over the lazy dog", "*GRUMBO")]
		[InlineData("the quick brown fox jumped over the lazy dog", "*OXFORD")]
		[InlineData("the quick brown fox jumped over the lazy dog", "*DISCORD")]
		[InlineData("the quick brown fox jumped over the lazy dog", "*ALGERNON")]
		[InlineData("the quick brown fox jumped over the lazy dog", "*HARRY")]
		[InlineData("the quick brown fox jumped over the lazy dog", "*HOPPER")]
		public void ShouldNotWildCardStartMatch(string content, string pattern)
		{
			Assert.False(DiscordWorker.FilterMatch(content, pattern));
		}
        
		[Theory]
		[InlineData("the quick brown fox jumped over the lazy dog", "TH*")]
		[InlineData("the quick brown fox jumped over the lazy dog", "QUI*")]
		[InlineData("the quick brown fox jumped over the lazy dog", "BR*")]
		[InlineData("the quick brown fox jumped over the lazy dog", "F*")]
		[InlineData("the quick brown fox jumped over the lazy dog", "JUMPE*")]
		[InlineData("the quick brown fox jumped over the lazy dog", "OV*")]
		[InlineData("the quick brown fox jumped over the lazy dog", "LAZ*")]
		[InlineData("the quick brown fox jumped over the lazy dog", "D*")]
		public void ShouldWildCardEndMatch(string content, string pattern)
		{
			Assert.True(DiscordWorker.FilterMatch(content, pattern));
		}
        
		[Theory]
		[InlineData("the quick brown fox jumped over the lazy dog", "MORK*")]
		[InlineData("the quick brown fox jumped over the lazy dog", "MINDI*")]
		[InlineData("the quick brown fox jumped over the lazy dog", "CHEERS*")]
		[InlineData("the quick brown fox jumped over the lazy dog", "FRAISER*")]
		[InlineData("the quick brown fox jumped over the lazy dog", "LARGEHADRONCOLLIDER*")]
		[InlineData("the quick brown fox jumped over the lazy dog", "MURICA*")]
		[InlineData("the quick brown fox jumped over the lazy dog", "FRANCE*")]
		[InlineData("the quick brown fox jumped over the lazy dog", "CANADA*")]
		public void ShouldNotWildCardEndMatch(string content, string pattern)
		{
			Assert.False(DiscordWorker.FilterMatch(content, pattern));
		}
	}
}