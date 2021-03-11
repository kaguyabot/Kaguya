using Discord;

namespace Kaguya.Internal.Extensions.DiscordExtensions
{
	public static class DiscordStringExtensions
	{
		public static string AsItalics(this string s) { return Format.Italics(s); }
		public static string AsUnderline(this string s) { return Format.Underline(s); }
		public static string AsBold(this string s) { return Format.Bold(s); }
		public static string AsBoldItalics(this string s) { return "***" + s + "***"; }
		public static string AsBoldUnderlined(this string s) { return "__**" + s + "**__"; }
		public static string AsEscapeUrl(this string s) { return Format.EscapeUrl(s); }

		/// <summary>
		///  Formats the strings as a url with description. Only usable in <see cref="EmbedBuilder" />s.
		///  Example: [description](url).
		/// </summary>
		/// <param name="description"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string AsUrl(this string description, string url) { return Format.Url(description, url); }

		public static string AsStrikethrough(this string s) { return Format.Strikethrough(s); }
		public static string AsCodeBlockSingleLine(this string s) { return "`" + s + "`"; }
		public static string AsCodeBlockMultiline(this string s) { return "```" + s + "```"; }

		/// <summary>
		///  Removes all markdown from the string.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string Sanitize(this string s) { return Format.Sanitize(s); }

		public static string AsBlueCode(this string s, string url = Global.TopGgUpvoteUrl) { return $"[`{s}`]({url})"; }
	}
}