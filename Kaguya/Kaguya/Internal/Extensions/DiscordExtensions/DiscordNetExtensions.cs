using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Interactivity;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Kaguya.Internal.Extensions.DiscordExtensions
{
	public static class DiscordNetExtensions
	{
		public static LogLevel ToLogLevel(this LogSeverity severity)
		{
			return severity switch
			{
				LogSeverity.Critical => LogLevel.Critical,
				LogSeverity.Error => LogLevel.Error,
				LogSeverity.Warning => LogLevel.Warning,
				LogSeverity.Info => LogLevel.Information,
				LogSeverity.Verbose => LogLevel.Trace,
				LogSeverity.Debug => LogLevel.Trace,
				_ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
			};
		}

		// CommandInfo extensions

		/// <summary>
		///  Returns the full name of the command. This is the group name + any additional commands in the group.
		///  Example: [Group("ban")] [Command("-u")] for a command returns "ban -u".
		/// </summary>
		/// <param name="cmdInfo"></param>
		/// <returns></returns>
		public static string GetFullCommandName(this CommandInfo cmdInfo) { return cmdInfo.Aliases[0]; }

		// User extensions

		/// <summary>
		///  Compares two <see cref="IUser" /> objects for equality, based only on user id.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static bool IsEqual(this IUser user, IUser other) { return user.Id == other.Id; }

		public static async Task<IUserMessage> SendEmbedAsync(this IUser user, EmbedBuilder embedBuilder)
		{
			return await user.SendMessageAsync(embed: embedBuilder.Build());
		}

		public static async Task<IUserMessage> SendEmbedAsync(this IUser user, Embed embed)
		{
			return await user.SendMessageAsync(embed: embed);
		}

		public static void SendEmbedWithDeletion(this InteractivityService interactivityService, ICommandContext context, Embed embed,
			TimeSpan deletionDelay)
		{
			interactivityService.DelayedSendMessageAndDeleteAsync(context.Channel, embed: embed, deleteDelay: deletionDelay);
		}

		public static bool AllShardsReady(this DiscordShardedClient client) { return client.Shards.Count == Global.ShardsReady.Count; }
	}
}