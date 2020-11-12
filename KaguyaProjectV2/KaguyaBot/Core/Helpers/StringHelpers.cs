namespace KaguyaProjectV2.KaguyaBot.Core.Helpers
{
    public static class StringHelpers
    {
        /// <summary>
        /// Formats the last character of a string, either adding an 's' or not, based
        /// on the <see cref="num"/> provided.
        ///
        /// Example: SFormat("pineapple", 3) returns "pineapples".
        /// Example: SFormat("pineapple", 1) returns "pineapple".
        ///
        /// This is useful if the num is variable, based on some other factor (loop count, etc.)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string SFormat(string s, int num)
        {
            if (num == 1)
                return s;
            
            return s + "s";
        }
    }
}