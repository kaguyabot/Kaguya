using Discord;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Administration
{
	[Module(CommandModule.Administration)]
	[Group("textchannel")]
	[Alias("tc", "txt", "t")]
	[RequireUserPermission(GuildPermission.ManageChannels)]
	[RequireBotPermission(GuildPermission.ManageChannels)]
	public class TextChannel : KaguyaBase<TextChannel>
	{
		private const int MAX_LENGTH = 100;
		// MUST ALL BE LOWER CASE!!
		private static readonly char[] _validChars = "1234567890abcdefghijklmnopqrstuvwxyz_ -".ToCharArray();
		private readonly ILogger<TextChannel> _logger;
		public TextChannel(ILogger<TextChannel> logger) : base(logger) { _logger = logger; }

		[Command("-create")]
		[Alias("-c")]
		[Summary("Creates a text channel with the given name.")]
		[Remarks("<name>")]
		[Example("example")]
		[Example("e-x-a-m-p-l-e")]
		[Example("e x a m p l e")]
		public async Task CreateTextChannelCommandAsync([Remainder]
			string name)
		{
			if (!IsValidTextChannelName(name, out string error))
			{
				await SendBasicErrorEmbedAsync(error);
				return;
			}

			ReplaceSpaces(ref name);

			try
			{
				var restChannel = await Context.Guild.CreateTextChannelAsync(name);

				await SendBasicSuccessEmbedAsync($"Successfully created channel {restChannel.Mention}.");
			}
			catch (Exception e)
			{
				await SendBasicErrorEmbedAsync("Failed to create text channel. Error:\n" + $"{e.Message}");
				_logger.LogDebug(e, $"Failed to create text channel in guild {Context.Guild.Id}.");
			}
		}

		[Command("-delete")]
		[Alias("-d")]
		[Summary("Deletes a text channel. You can pass in the ID, name, or link.")]
		[Remarks("<channel>")]
		[Example("80055699999990819723 (ID of #example)")]
		[Example("#example")]
		[Example("example")]
		public async Task DeleteTextChannelCommandAsync(ITextChannel channel)
		{
			try
			{
				await channel.DeleteAsync();
			}
			catch (Exception e)
			{
				await SendBasicErrorEmbedAsync("Failed to delete text channel. Error:\n" + $"{e.Message}");
				_logger.LogDebug(e, $"Failed to delete text channel in guild {Context.Guild.Id}.");
			}

			await SendBasicSuccessEmbedAsync("Successfully deleted the text channel.");
		}

		[Command("-rename")]
		[Alias("-r")]
		[Summary("Renames the text channel to the desired name.")]
		[Remarks("<channel> <new name>")]
		[Example("#my-channel my new name")]
		public async Task RenameTextChannelCommandAsync(ITextChannel channel, [Remainder]
			string newName)
		{
			if (!IsValidTextChannelName(newName, out string error))
			{
				await SendBasicErrorEmbedAsync(error);
				return;
			}

			ReplaceSpaces(ref newName);

			try
			{
				await channel.ModifyAsync(x => x.Name = newName);
			}
			catch (Exception e)
			{
				await SendBasicErrorEmbedAsync($"Failed to rename text channel. Error:\n{e.Message}");
				_logger.LogDebug(e, $"Failed to rename text channel in guild {Context.Guild.Id}.");
			}

			await SendBasicSuccessEmbedAsync("Renamed " + $"#{channel.Name}".AsBold() + $" to {newName.AsBold()}");
		}

		public static bool IsValidTextChannelName(string name, out string error)
		{
			ReplaceSpaces(ref name);

			if (name.Length > MAX_LENGTH)
			{
				error = "The maximum amount of characters in a " + "channel name is 100.";

				return false;
			}

			if (!name.Any())
			{
				error = "You cannot specify an empty channel name.";

				return false;
			}

			if (name.ToLower().Any(x => !_validChars.Contains(x)))
			{
				error = "Your channel name contains invalid characters. " +
				        "Please type the new channel name, like this: " +
				        "`my channel` or `my-channel`. Only alphanumeric latin characters " +
				        "are permitted (no symbols).";

				return false;
			}

			error = null;
			return true;
		}

		private static void ReplaceSpaces(ref string channelName) { channelName = channelName.Replace(' ', '-'); }
	}
}