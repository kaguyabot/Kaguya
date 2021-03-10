using Discord;
using Discord.Commands;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Nsfw
{
	[Restriction(ModuleRestriction.PremiumUser | ModuleRestriction.PremiumServer)]
	[Module(CommandModule.Nsfw)]
	[Group("nsfw")]
	[Alias("n")]
	public class Nsfw : KaguyaBase<Nsfw>
	{
		private static readonly string[] _blacklistedTags =
		{
			"loli",
			"shota",
			"scat",
			"gore",
			"blood"
		};
		private readonly KaguyaServerRepository _kaguyaServerRepository;

		public Nsfw(ILogger<Nsfw> logger, KaguyaServerRepository kaguyaServerRepository) : base(logger)
		{
			_kaguyaServerRepository = kaguyaServerRepository;
		}

		[Priority(1)]
		[RequireNsfw]
		[Command(RunMode = RunMode.Async)]
		[Summary("Sends NSFW images into the given chat channel, if previously permitted " +
		         "by server administration.\n\n" +
		         "An optional list of tags may be provided. Please note that some tags are blacklisted." +
		         " The less tags provided, the " +
		         "more likely you are to find a successful result.\n\n" +
		         "Separate tags with spaces. Limited to 10, image count is 3 by default.")]
		[Remarks("[count] [tags]")] // Delete if no remarks needed.
		[Example("")]
		[Example("3")]
		[Example("5 swimsuit")]
		public async Task SendNsfwCommandAsync(int count = 3) { await ExecuteNsfwAsync(count, new List<string>()); }

		[Priority(0)]
		[RequireNsfw]
		[Command(RunMode = RunMode.Async)]
		public async Task SendNsfwCommandAsync(int count,
			[Remainder]
			string tags)
		{
			string[] splits = tags.Split(' ');

			foreach (string tag in splits)
			{
				string curTag = tag.ToLower();

				foreach (string blacklistedTag in _blacklistedTags)
				{
					string tempBlTag = blacklistedTag.ToLower();
					if (curTag.Contains(tempBlTag))
					{
						await SendBasicErrorEmbedAsync("One or more of your tags are blacklisted.");

						return;
					}
				}
			}

			await ExecuteNsfwAsync(count, splits);
		}

		private async Task ExecuteNsfwAsync(int count, IList<string> tags)
		{
			if (count > 10)
			{
				await SendBasicErrorEmbedAsync("You can only send 10 images at a time.");

				return;
			}

			var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);

			if (!server.IsNsfwAllowed)
			{
				await SendBasicErrorEmbedAsync("Sorry, this server's administration hasn't allowed " +
				                               "NSFW command usage. Please contact them and ask them " +
				                               $"to execute the `{server.CommandPrefix}nsfw -toggle` command.");

				return;
			}

			var urls = await NsfwService.GetHentaiUrlsAsync(count, tags.ToList());

			if (!urls.Any())
			{
				await SendBasicErrorEmbedAsync("Your search returned no results.");

				return;
			}

			foreach (string url in urls)
			{
				string urlEnd = url.Split('/').Last();
				using (var client = new WebClient())
				{
					byte[] data = await client.DownloadDataTaskAsync(url);

					using (var m = new MemoryStream(data))
					{
						try
						{
							await Context.Channel.SendFileAsync(m, $"kaguya_nsfw_{urlEnd}");
						}
						catch (Exception)
						{
							// We can safely ignore these exceptions.
						}
					}
				}
			}
		}

		[RequireUserPermission(GuildPermission.Administrator)]
		[Command("-toggle")]
		[Summary("Toggles the nsfw module for this server on or off.")]
		public async Task NsfwToggleCommand()
		{
			var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
			if (server.IsNsfwAllowed)
			{
				server.IsNsfwAllowed = false;
				server.NsfwAllowedId = null;
				server.NsfwAllowanceTime = null;

				await SendBasicSuccessEmbedAsync("You have successfully disabled the nsfw " +
				                                 "command set for this server.");
			}
			else
			{
				server.IsNsfwAllowed = true;
				server.NsfwAllowedId = Context.User.Id;
				server.NsfwAllowanceTime = DateTimeOffset.Now;

				await SendBasicSuccessEmbedAsync("You have successfully enabled the nsfw " +
				                                 "command set for this server.");
			}

			await _kaguyaServerRepository.UpdateAsync(server);
		}
	}
}