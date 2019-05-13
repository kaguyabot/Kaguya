using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Kaguya.Core.Command_Handler.EmbedHandlers
{
    public class GlobalCommandResponses
    {

        /// <summary>
        /// Builds an embedded message and automatically sends it into the context's channel. This will also send a ConsoleCommandLog for a successfully executed command.
        /// </summary>
        /// <param name="context">SocketCommandContext of the command.</param>
        /// <param name="timespan">This will always be stopWatch.ElapsedMilliseconds(). This is the time it takes for the command to execute, in milliseconds.</param>
        /// <param name="title">Title of the embedded message.</param>
        /// <param name="description">Description of the embedded message.</param>
        /// <param name="footer">Footer of the embedded message.</param>
        /// <returns></returns>
        public static async Task CreateCommandResponse(SocketCommandContext context, long timespan, string title = null, string description = null, string footer = null)
        {
            Logger logger = new Logger();
            var embed = await Task.Run(() => (new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithFooter(footer)
                .WithColor(252, 132, 255).Build())); //Pink
            await context.Channel.SendMessageAsync(embed: embed.ToEmbedBuilder().Build());
            logger.ConsoleCommandLog(context, timespan);
        }

        /// <summary>
        /// Creates an embedded error message to be sent in chat.
        /// </summary>
        /// <param name="context">SocketCommandContext of the command. This will also create a command error and a ConsoleCommandLog for a failed command.</param>
        /// <param name="timespan"></param>
        /// <param name="cmdError"></param>
        /// <param name="errorMsg"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="footer"></param>
        /// <returns></returns>
        public static async Task CreateCommandError(SocketCommandContext context, long timespan, CommandError cmdError, string errorMsg, string title = null, string description = null, string footer = null)
        {
            Logger logger = new Logger();
            var embed = await Task.Run(() => (new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithFooter(footer)
                .WithColor(255, 0, 0).Build())); //Pink
            await context.Channel.SendMessageAsync(embed: embed.ToEmbedBuilder().Build());
            logger.ConsoleCommandLog(context, timespan, cmdError, errorMsg);
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
            Logger logger = new Logger();
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

            logger.ConsoleStatusAdvisory("Automatic bug report created.");

            return;
        }
    }
}
