using System.Text.RegularExpressions;

namespace Kaguya.Discord.Parsers
{
    public class UrlParser
    {
        private readonly string _input;
        private static readonly Regex _urlRegex = new Regex(@"[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
        private static readonly Regex _httpUrlRegex = new Regex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
        public UrlParser(string input) { _input = input; }

        /// <summary>
        /// Determines whether the input provided in this class's constructor contains a URL (does
        /// not check for HTTPS).
        /// </summary>
        /// <returns></returns>
        public bool IsUrl() => _urlRegex.IsMatch(_input);

        /// <summary>
        /// Determines whether the input provided in this class's constructor contains a HTTP or HTTPS url.
        /// </summary>
        /// <returns></returns>
        public bool IsHttpUrl() => _httpUrlRegex.IsMatch(_input);
    }
}