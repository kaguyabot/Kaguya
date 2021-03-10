using Discord;
using Discord.Commands;
using Humanizer;
using Interactivity;
using Interactivity.Pagination;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Configuration
{
	[Module(CommandModule.Configuration)]
	[Group("admin")]
	[RequireUserPermission(GuildPermission.Administrator)]
	[RequireBotPermission(GuildPermission.Administrator)]
	public class AdminHistory : KaguyaBase<AdminHistory>
	{
		private readonly AdminActionRepository _adminActionRepository;
		private readonly InteractivityService _interactivityService;
		private readonly KaguyaServerRepository _kaguyaServerRepository;

		public AdminHistory(ILogger<AdminHistory> logger,
			AdminActionRepository adminActionRepository,
			KaguyaServerRepository kaguyaServerRepository,
			InteractivityService interactivityService) : base(logger)
		{
			_adminActionRepository = adminActionRepository;
			_kaguyaServerRepository = kaguyaServerRepository;
			_interactivityService = interactivityService;
		}

		[Restriction(ModuleRestriction.PremiumServer)]
		[Command("-history", RunMode = RunMode.Async)]
		[Alias("-hist")]
		[Summary(
			"Displays administration action history for events in this server. Filter down the results using the " +
			"arguments described below. If no filters are provided, all items will be displayed.\n\n" +
			"**Filters:**\n" +
			"`ban`, `unban`, `kick`, `warn`, `unwarn`, `mute`, `unmute`\n" +
			"**Arguments:**\n" +
			"`-f <filter type> [...]`\n" +
			"`--se` - Show expired entries (only applicable to mute, ban, and shadowban)\n" +
			"`--sh` - Show hidden entries\n\n" +
			"")]
		[Remarks("[-f <filter type> [...]] [--se] [--sh]")]
		[Example("")]
		[Example("-f kick ban shadowban")]
		[Example("-f mute --sh --se")]
		[Example("-f warn --sh")]
		public async Task AdminHistoryCommand(params string[] args)
		{
			bool showExpired = ShowExpired(args);
			bool showHidden = ShowHidden(args);

			string[] validFilters = AdminAction.AllActions;
			string[] durationFilters =
			{
				AdminAction.BanAction,
				AdminAction.MuteAction,
				AdminAction.ShadowbanAction
			};

			// TODO: Throw invalid msg if args are provided after --se / --sh. This does not break any functionality, however.

#region Modifiers
			bool showAll = ShowAll(args);
			string[] userInputFilters = showAll ? validFilters : args[1..].TakeWhile(x => !x.Contains("--")).ToArray();

			bool invalid = InvalidInput(userInputFilters, validFilters);

			if (invalid)
			{
				await SendBasicErrorEmbedAsync("You provided an invalid filter. Valid filters are " +
				                               validFilters.Humanize(x => $"`{x}`"));

				return;
			}
#endregion

			var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
			var unFilteredCollection = (await _adminActionRepository.GetAllAsync(server.ServerId, showHidden)).ToList();
			var collection = new List<AdminAction>();
			foreach (string filter in userInputFilters)
			{
				if (showAll)
				{
					break;
				}

				var toAdd = unFilteredCollection.Where(x =>
					x.Action.Equals(filter, StringComparison.OrdinalIgnoreCase));

				collection.AddRange(toAdd);
			}

			if (showAll)
			{
				collection = unFilteredCollection;
			}

			if (!showExpired)
			{
				collection.RemoveAll(x => x.Expiration < DateTimeOffset.Now && durationFilters.Contains(x.Reason));
			}

			const int perPage = 10;
			int pageCount = (collection.Count + (perPage - 1)) / perPage;

			var pages = new PageBuilder[pageCount];

			for (int i = 0; i < pages.Length; i++)
			{
				// We subtract 1 because we want 20 per page, but ref the PER_PAGE variable as an index.
				int minIndex = pages.Length == 1 ? 0 : i * perPage;
				int maxRange = pages.Length == 1 ? Math.Abs(collection.Count - perPage) : (i + 1) * perPage;

				if (maxRange >= collection.Count)
				{
					maxRange = collection.Count;
				}

				maxRange -= minIndex;

				var curCollection = collection.GetRange(minIndex, maxRange);

				var page = new PageBuilder().WithTitle($"Administration History: {Context.Guild.Name}")
				                            .WithColor(KaguyaColors.LightYellow);

				page.Description = GetPageDescription(curCollection, durationFilters, server);
				pages[i] = page;
			}

			if (pages.Length == 0)
			{
				await SendBasicErrorEmbedAsync("No records found.");

				return;
			}

			var paginator = new StaticPaginatorBuilder().WithPages(pages)
			                                            .WithUsers(Context.User)
			                                            .WithFooter(PaginatorFooter.Users | PaginatorFooter.PageNumber)
			                                            .Build();

			await _interactivityService.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(30));
		}

		private string GetPageDescription(List<AdminAction> curCollection,
			string[] durationFilters,
			KaguyaServer server)
		{
			var sb = new StringBuilder();
			foreach (var current in curCollection)
			{
				bool hasDuration =
					durationFilters.Any(x => x.Equals(current.Action, StringComparison.OrdinalIgnoreCase));

				string moderatorUser = Context.Guild.GetUser(current.ModeratorId)?.Mention ??
				                       current.ModeratorId.ToString();

				string actionedUser = Context.Guild.GetUser(current.ActionedUserId)?.Mention ??
				                      current.ActionedUserId.ToString();

				string reason = current.Reason ?? "<No reason provided>";
				string hiddenString = current.IsHidden ? " " + "[Hidden]".AsBold() + " " : default;
				string toAppend =
					$"ID: {current.Id}.{hiddenString} Type: {current.Action.AsBold()} | Moderator: {moderatorUser} | User: {actionedUser} |\nReason: {reason.AsBold()}";

				if (hasDuration)
				{
					string expiration = !current.Expiration.HasValue ? "Permanent" :
						current.Expiration.Value < DateTimeOffset.Now ? "Expired" :
						(current.Expiration - DateTimeOffset.Now).Value.HumanizeTraditionalReadable();

					toAppend += $" | Duration: {expiration.AsBold()}";
				}

				sb.AppendLine(toAppend);
			}

			sb.AppendLine(
				$"\nHide entries using the `{server.CommandPrefix}admin -hide <id>` command. The id is found next to the entry in this list.");

			return sb.ToString();
		}

		private bool InvalidInput(string[] userInputFilters, string[] validFilters)
		{
			bool invalid = false;
			foreach (string input in userInputFilters)
			{
				if (!validFilters.Contains(input, StringComparer.OrdinalIgnoreCase))
				{
					invalid = true;

					break;
				}
			}

			return invalid;
		}

		private bool ShowAll(string[] args)
		{
			return !args.Any() || !args[0].Equals("-f", StringComparison.OrdinalIgnoreCase);
		}

		private bool ShowExpired(string[] args)
		{
			return args.Any(x => x.Equals("--se", StringComparison.OrdinalIgnoreCase));
		}

		private bool ShowHidden(string[] args)
		{
			return args.Any(x => x.Equals("--sh", StringComparison.OrdinalIgnoreCase));
		}
	}
}