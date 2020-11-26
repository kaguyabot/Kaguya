using System;
using Kaguya.Discord.Parsers;
using Xunit;

namespace KaguyaTests
{
	public class TestTimeParser
	{
		[Theory]
		[InlineData("1s")]
		[InlineData("1m")]
		[InlineData("1h")]
		[InlineData("1d")]
		public void ShouldParseSingle(string input)
		{
			var p = new TimeParser(input);
			Assert.True(IsValid(p.ParseTime()));
		}
		
		[Theory]
		[InlineData("1s1m")]
		[InlineData("1s1h")]
		[InlineData("1s1d")]
		[InlineData("1s1m1h")]
		[InlineData("1s1m1h1d")]
		[InlineData("1d1h1m1s")]
		[InlineData("1m1h1d1s")]
		public void ShouldParseMultiple(string input)
		{
			var p = new TimeParser(input);
			Assert.True(IsValid(p.ParseTime()));
		}
		
		[Theory]
		[InlineData("35d21h134m99s")]
		public void ShouldParseExact(string input)
		{
			var p = new TimeParser(input);
			var pTime = p.ParseTime();
			
			Assert.True(IsValid(pTime) && pTime == new TimeSpan(35, 21, 134, 99));
		}

		private bool IsValid(TimeSpan ts) => ts != TimeSpan.Zero;
	}
}