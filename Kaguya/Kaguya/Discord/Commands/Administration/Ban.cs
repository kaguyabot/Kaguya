using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Humanizer;
using Humanizer.Localisation;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Parsers;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Administration
{
	[Module(CommandModule.Administration)]
	[Group("ban")]
	[Alias("b")]
	[RequireUserPermission(GuildPermission.BanMembers)]
	[RequireBotPermission(GuildPermission.BanMembers)]
	public class Ban : KaguyaBase<Ban>
	{
		private readonly AdminActionRepository _adminActionRepository;
		private readonly KaguyaServerRepository _kaguyaServerRepository;
		private readonly ILogger<Ban> _logger;

		public Ban(ILogger<Ban> logger, KaguyaServerRepository kaguyaServerRepository,
			AdminActionRepository adminActionRepository) : base(logger)
		{
			_logger = logger;
			_kaguyaServerRepository = kaguyaServerRepository;
			_adminActionRepository = adminActionRepository;
		}

		[Command]
		[Summary("Permanently bans a user from the server.")]
		[Remarks("<user> [reason]")]
		public async Task BanCommand(SocketGuildUser user, [Remainder]
			string reason = null)
		{
			var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
			try
			{
				var adminAction = new AdminAction
				{
					ServerId = Context.Guild.Id,
					ModeratorId = Context.User.Id,
					ActionedUserId = user.Id,
					Action = AdminAction.BanAction,
					Reason = reason,
					Expiration = null,
					Timestamp = DateTimeOffset.Now
				};

				// Bans the user *and* updates the server admin actions in DB.
				await BanAsync(user, adminAction, reason);
				await SendBasicSuccessEmbedAsync($"Banned **{user}**.");
				// TODO: Trigger ban event.
			}
			catch (Exception e)
			{
				string errorString = new StringBuilder()
				                     .AppendLine($"{Context.User.Mention} Failed to ban user {user.ToString().AsBold()}.")
				                     .AppendLine("Do I have enough permissions?".AsItalics())
				                     .AppendLine("This error can also occur if the user you are trying to ban has more permissions than me."
					                     .AsItalics())
				                     .AppendLine("Ensure my role is also at the top of the role heirarchy, then try again.".AsItalics())
				                     .Append($"Error: {e.Message.AsBold()}")
				                     .ToString();

				await SendBasicErrorEmbedAsync(errorString);

				_logger.LogDebug(e, "Exception encountered with ban in guild " + server.ServerId);
			}
		}

		private async Task BanAsync(IGuildUser user, AdminAction action, string reason)
		{
			var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
			server.TotalAdminActions++;

			await _adminActionRepository.InsertAsync(action);
			await _kaguyaServerRepository.UpdateAsync(server);

			await user.BanAsync(reason: reason);
		}

		[Command("-u")]
		[Summary("Unbans the user from the server.")]
		[Remarks("<user id> [reason]")]
		public async Task CommandUnban(ulong id, [Remainder]
			string reason = null)
		{
			try
			{
				await Context.Guild.RemoveBanAsync(id);

				var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
				server.TotalAdminActions++;

				var adminAction = new AdminAction
				{
					ServerId = Context.Guild.Id,
					ModeratorId = Context.User.Id,
					ActionedUserId = id,
					Action = AdminAction.UnbanAction,
					Reason = reason,
					Expiration = null,
					Timestamp = DateTimeOffset.Now
				};

				await _adminActionRepository.InsertAsync(adminAction);

				// Try to get the name of the user for display, if they exist.
				var actionedUser = await Context.Guild.GetBanAsync(id);

				await _kaguyaServerRepository.UpdateAsync(server);
				await SendBasicSuccessEmbedAsync(
					$"Unbanned user with ID {actionedUser?.User.ToString().AsBold() ?? id.ToString().AsBold()}.");

				// TODO: Trigger unban event.
			}
			catch (Exception e)
			{
				if (e is HttpException httpEx)
				{
					// The user could not be found in the guild's ban list.
					if (httpEx.HttpCode == HttpStatusCode.NotFound)
					{
						await SendBasicErrorEmbedAsync("This user is not banned.");
						return;
					}
				}

				await SendBasicErrorEmbedAsync($"Failed to unban user with id {id.ToString().AsBold()}. Error: {e.Message.AsBold()}");

				_logger.LogDebug(e, $"Exception encountered with ban in guild {Context.Guild}.");
			}
		}

		[Command("-t")]
		[Summary("Temporarily bans the user from the server for the time specified.")]
		[Remarks("<user> <duration> [reason]")]
		public async Task CommandTempban(SocketGuildUser user, string timeString, [Remainder]
			string reason = null)
		{
			var timeParser = new TimeParser(timeString);
			var parsedTime = timeParser.ParseTime();
			if (parsedTime == TimeSpan.Zero)
			{
				await SendBasicErrorEmbedAsync($"Failed to temp-ban user **{user}**.\n" +
				                               $"`{timeString}` could not be parsed into a duration.");

				return;
			}

			try
			{
				var adminAction = new AdminAction
				{
					ServerId = Context.Guild.Id,
					ModeratorId = Context.User.Id,
					ActionedUserId = user.Id,
					Action = AdminAction.BanAction,
					Reason = reason,
					Expiration = DateTimeOffset.Now + parsedTime,
					Timestamp = DateTimeOffset.Now,
					HasTriggered = false // We specify this value if the user is temporarily actioned. Otherwise, leave it null.
				};

				await BanAsync(user, adminAction, reason);

				string humanizedDuration = parsedTime.Humanize(3, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Year);
				await SendBasicSuccessEmbedAsync(
					$"Successfully tempbanned user {user.ToString().AsBold()} for {humanizedDuration.AsBold()}.");

				// TODO: Ensure service watches for new temporary bans and unbans when the time expires.
			}
			catch (Exception e)
			{
				_logger.LogDebug(e, $"Failed to tempban user {user.Id} in guild {Context.Guild.Id}.");
				await SendBasicErrorEmbedAsync($"Failed to tempban user **{user}**. Reason: {e.Message.AsBold()}");
			}
		}
	}
}