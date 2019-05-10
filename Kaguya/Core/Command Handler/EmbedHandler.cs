using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Kaguya.Core.CommandHandler
{
    public static class EmbedHandler
    {
        /// <summary>
        /// Builds a basic pink embed in chat. This is generally used for successful command responses.
        /// </summary>
        /// <param name="title">Title of the message to be embedded.</param>
        /// <param name="description">Description of the message to be embedded.</param>
        /// <param name="footer">Optional footer to include at the bottom of the embed.</param>
        /// <returns></returns>
        public static async Task<Embed> CreateBasicEmbed(string title, string description, string footer = null)
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
        public static async Task<Embed> CreateMusicEmbed(string title, string description, string footer = null)
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
        public static async Task<Embed> CreateErrorEmbed(string source, string error, string footer = null)
        {
            var embed = await Task.Run(() => new EmbedBuilder()
                .WithTitle($"Error Source: {source}")
                .WithDescription($"**Error: {error}**")
                .WithFooter(footer)
                .WithColor(Color.Red).Build());
            return embed;
        }

        /// <summary>
        /// Creates an automatic bug report that is then delivered to the Kaguya Support Server's #bug-reports channel.
        /// </summary>
        /// <param name="source">Source of the bug/error in question.</param>
        /// <param name="bug">The actual bug or the bug's message.</param>
        /// <param name="footer">Optional footer to include at the bottom of the embed.</param>
        /// <returns></returns>
        public static async Task CreateAutomaticBugReport(string source, string bug, string footer = null)
        {
            var _client = Global.Client;
            var embed = await Task.Run(() => new EmbedBuilder()
            .WithTitle("Automatic Bug Report")
            .WithDescription($"Source: `{source}`" +
            $"\nBug: `{bug}`")
            .WithFooter(footer)
            .WithTimestamp(DateTime.Now)
            .WithColor(Color.Red));

            var bugChannel = _client.GetChannel(547448889620299826); //Kaguya support server #bugs channel.
            await (bugChannel as ISocketMessageChannel).SendMessageAsync(embed: embed.Build()); //Sends first embed to bug report channel.
            return;
        }
    }
}
