#region This really shouldn't be touched. Creates static embed replies that are then sent in a ReplyAsync method...ONLY used for Music.cs.
#endregion

using Discord;
using System.Threading.Tasks;

namespace Kaguya.Core.CommandHandler
{
    public class StaticMusicEmbedHandler
    {
        /// <summary>
        /// These are only used for the MusicService.cs class and NOTHING else. All command responses should pass in Context as the first parameter!
        /// </summary>
        /// <param name="title">Title of the message to be embedded.</param>
        /// <param name="description">Description of the message to be embedded.</param>
        /// <param name="footer">Optional footer to include at the bottom of the embed.</param>
        /// <returns></returns>
        public static async Task<Discord.Embed> CreateBasicEmbed(string title = null, string description = null, string footer = null)
        {
            var embed = await Task.Run(() => (new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithFooter(footer)
                .WithColor(252, 132, 255).Build())); //Pink
            return embed;
        }

        /// <summary>
        /// Create's an embedded message for the bot's music features. This is generally only used for successful responses to music commands.
        /// The color of this embed is always light blue.
        /// </summary>
        /// <param name="title">Title of the embedded message.</param>
        /// <param name="description">Description of the embedded message.</param>
        /// <param name="footer">Optional footer to include at the bottom of the embed.</param>
        /// <returns></returns>
        public static async Task<Discord.Embed> CreateMusicEmbed(string title, string description, string footer = null)
        {
            var embed = await Task.Run(() => (new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithFooter(footer)
                .WithColor(0, 255, 255) //Light Blue
                .WithCurrentTimestamp().Build()));
            return embed;
        }

        /// <summary>
        /// Create's an error embed with a source, error, and optional footer. This is generally used for invalid command executions, command errors, 
        /// exceptions, etc.
        /// </summary>
        /// <param name="source">Source of the error.</param>
        /// <param name="error">The error that is being thrown.</param>
        /// <param name="footer">Optional footer to include at the bottom of the embed.</param>
        /// <returns></returns>
        public static async Task<Discord.Embed> CreateErrorEmbed(string source, string error, string footer = null)
        {
            var embed = await Task.Run(() => new EmbedBuilder()
                .WithTitle($"Error Source: {source}")
                .WithDescription($"**Error: {error}**")
                .WithFooter(footer)
                .WithColor(Color.Red).Build());
            return embed;
        }
    }
}
