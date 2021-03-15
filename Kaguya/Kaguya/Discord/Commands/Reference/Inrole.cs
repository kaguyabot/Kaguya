using Discord;
using Discord.Commands;
using Interactivity;
using Interactivity.Pagination;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Reference
{
	[Module(CommandModule.Reference)]
	[Group("inrole")]
	[Alias("in")]
	public class Inrole : KaguyaBase<Inrole>
	{
		private const int PAGE_SIZE = 25;
		private readonly InteractivityService _interactivityService;

		public Inrole(ILogger<Inrole> logger, InteractivityService interactivityService) : base(logger)
		{
			_interactivityService = interactivityService;
		}

		[Command(RunMode = RunMode.Async)]
		[Summary("Displays a list of users who have the specified role.")]
		[Remarks("<role>")]
		[Example("@Penguins")]
		[Example("Penguins")]
		[Example("\"Penguins with spaces!\"")]
		public async Task InroleCommandAsync(IRole role)
		{
			if (role.Equals(Context.Guild.EveryoneRole))
			{
				await SendBasicErrorEmbedAsync($"Sorry, the {role.Mention} role isn't supported by this command!");
				return;
			}

			var usersWithRole = Context.Guild.Users.Where(x => x.Roles.Contains(role))
			                           .OrderByDescending(x => x.Nickname ?? x.Username)
			                           .ToList();

			if (!usersWithRole.Any())
			{
				await SendBasicEmbedAsync("No users have this role.", Color.LightOrange);
				return;
			}

			int memberCount = usersWithRole.Count;
			string footer = memberCount == 1
				? $"There is {usersWithRole.Count} member with this role."
				: $"There are {usersWithRole.Count:N0} members with this role.";

			if (usersWithRole.Count <= PAGE_SIZE)
			{
				var listSb = new StringBuilder($"Users with role {role.Mention}\n".AsBoldUnderlined());
				foreach (var guildUser in usersWithRole)
				{
					listSb.AppendLine(guildUser.Mention);
				}

				var embed = GetBasicEmbedBuilder(listSb.ToString(), KaguyaColors.IceBlue, false).WithFooter(footer);

				await SendEmbedAsync(embed);
			}
			else
			{
				int pageCount = (int) Math.Ceiling(memberCount / (float) PAGE_SIZE);
				var pages = new PageBuilder[pageCount];

				for (int i = 0; i < pageCount; i++)
				{
					var pageBuilder = new PageBuilder();
					var pageDescription = new StringBuilder($"Users with role {role.Mention}\n".AsBoldUnderlined());

					for (int j = 0; j < PAGE_SIZE; j++)
					{
						int accessIdx = (i * PAGE_SIZE) + j;
						if (accessIdx >= memberCount)
						{
							break;
						}

						pageDescription.AppendLine(usersWithRole.ElementAt(accessIdx).Mention);
					}

					pageDescription.AppendLine();
					pageDescription.Append($"Total members with role: {memberCount:N0}");

					pageBuilder.WithDescription(pageDescription.ToString()).WithColor(KaguyaColors.IceBlue);

					pages[i] = pageBuilder;
				}

				var paginator = new StaticPaginatorBuilder().WithPages(pages)
				                                            .WithUsers(Context.User)
				                                            .WithFooter(PaginatorFooter.Users | PaginatorFooter.PageNumber)
				                                            .Build();

				try
				{
					await _interactivityService.SendPaginatorAsync(paginator, Context.Channel);
				}
				catch (Exception e)
				{
					//
				}
			}
		}
	}
}