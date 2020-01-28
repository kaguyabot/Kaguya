using Discord;

namespace KaguyaProjectV2.KaguyaBot.Core.Global
{
    /// <summary>
    /// A static class containing helpful objects, such as <see cref="Emoji"/>s that are referenced often.
    /// </summary>
    public static class HelpfulObjects
    {
        /// <summary>
        /// Returns an Emoji[8] containing the numeric emojis 1-9.
        /// EmojisOneThroughNine[0] returns the Emoji for "1".
        /// </summary>
        /// <returns></returns>
        public static Emoji[] EmojisOneThroughNine()
        {
            return new Emoji[] { new Emoji("1⃣"), new Emoji("2⃣"), new Emoji("3⃣"),
                new Emoji("4⃣"),  new Emoji("5⃣"),  new Emoji("6⃣"),  new Emoji("7⃣"),
                new Emoji("8⃣"),  new Emoji("9⃣")
            };
        }

        public const string KAGUYA_STORE_URL = @"https://the-kaguya-project.myshopify.com/";

        /// <summary>
        /// The default emoji with reaction replies that use a check mark.
        /// </summary>
        /// <returns></returns>
        public static Emoji CheckMarkEmoji()
        {
            return new Emoji("✅");
        }

        /// <summary>
        /// The "no-entry" emoji. Default "cancel" emoji reaction for reaction replies.
        /// </summary>
        /// <returns></returns>
        public static Emoji NoEntryEmoji()
        {
            return new Emoji("⛔");
        }

    }
}
