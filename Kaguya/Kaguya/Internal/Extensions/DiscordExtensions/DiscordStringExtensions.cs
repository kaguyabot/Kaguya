using Discord;

namespace Kaguya.Internal.Extensions.DiscordExtensions
{
	public static class DiscordStringExtensions
	{
		public static string AsItalics(this string s) => Format.Italics(s);
		public static string AsUnderline(this string s) => Format.Underline(s);
		public static string AsBold(this string s) => Format.Bold(s);
		public static string AsBoldItalics(this string s) => "***" + s + "***";
		public static string AsBoldUnderlined(this string s) => "__**" + s + "**__";
		public static string AsEscapeUrl(this string s) => Format.EscapeUrl(s);
		/// <summary>
		/// Formats the strings as a url with description. Only usable in <see cref="EmbedBuilder"/>s.
		/// Example: [description](url).
		/// </summary>
		/// <param name="description"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string AsUrl(this string description, string url) => Format.Url(description, url);
		public static string AsStrikethrough(this string s) => Format.Strikethrough(s);
		public static string AsCodeBlockSingleLine(this string s) => "`" + s + "`";
		public static string AsCodeBlockMultiline(this string s) => "```" + s + "```";
		
		/// <summary>
		/// Removes all markdown from the string.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string Sanitize(this string s) => Format.Sanitize(s);
	}
}