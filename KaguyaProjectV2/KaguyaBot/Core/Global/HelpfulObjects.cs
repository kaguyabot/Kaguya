using Discord;

namespace KaguyaProjectV2.KaguyaBot.Core.Global
{
    public class HelpfulObjects
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
    }
}
