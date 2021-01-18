using System;

namespace Kaguya.Discord.Parsers
{
    public class UriParser
    {
        /// <summary>
        /// Determines whether the input provided contains a URI.
        /// </summary>
        /// <returns></returns>
        public static bool ContainsUri(string input)
        {
            var splits = input.Split(' ');
            foreach (string s in splits)
            {
                if (Uri.TryCreate(s, UriKind.Absolute, out var result))
                {
                    if (result.IsAbsoluteUri)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}