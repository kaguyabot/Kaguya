using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Interactivity;
using Interactivity.Pagination;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Attributes;
using Kaguya.Discord.Attributes.Enums;
using Kaguya.Discord.DiscordExtensions;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands.Administration
{
    [Module(CommandModule.Administration)]
    [Group("filter")]
    [Alias("f")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    [RequireBotPermission(GuildPermission.ManageMessages)]
    public class Filter : KaguyaBase<Filter>
    {
        private readonly WordFilterRepository _fwRepo;
        private readonly InteractivityService _interactivityService;
        private readonly ILogger<Filter> _logger;

        protected Filter(ILogger<Filter> logger, WordFilterRepository fwRepo, InteractivityService interactivityService) : base(logger)
        {
            _logger = logger;
            _fwRepo = fwRepo;
            _interactivityService = interactivityService;
        }

        [Command(RunMode = RunMode.Async)]
        [Summary("Displays the currently filtered words.")]
        public async Task CommandViewFilter()
        {
            FilteredWord[] filter = await _fwRepo.GetAllForServerAsync(Context.Guild.Id, true);

            if (filter.Length == 0)
            {
                await SendBasicErrorEmbedAsync("The word filter is empty.");

                return;
            }

            int numPages = (int) Math.Floor((double) (filter.Length + 24) / 25);
            var pages = new PageBuilder[numPages];

            int index = 0;
            for (int i = 0; i < numPages; i++)
            {
                PageBuilder page = new PageBuilder()
                                   .WithTitle("Word Filter")
                                   .WithColor(Color.Magenta);

                var descSb = new StringBuilder();

                for (int j = 0; j < 25; j++)
                {
                    if (index == filter.Length)
                        break;

                    var match = filter.ElementAt(index);
                    
                    descSb.AppendLine(match.Word + $" | Punishment: {match.FilterReactionString}");
                    index++;
                }

                page.Description = descSb.ToString();

                pages[i] = page;
            }

            Paginator paginator = new StaticPaginatorBuilder()
                                  .WithPages(pages)
                                  .WithUsers(Context.User)
                                  .WithFooter(PaginatorFooter.PageNumber)
                                  .Build();

            await _interactivityService.SendPaginatorAsync(paginator, Context.Channel);
        }

        [Priority(1)]
        [Command("-a", RunMode = RunMode.Async)]
        [Summary("Adds a word to the filter. Append a `*` character at the end to have the word " +
                 "be marked as a wildcard. Wildcards are detected if any portion of a user's message " +
                 "contains the filtered keyword. A non-wildcard filtered word will only be filtered if " +
                 "any word in the user's message matches a word in the filter exactly.\n\n" +
                 "Example: `filter -a tacos*` will detect `tacos`, `tacosopweijfgwoeifj`, `tacos+burritos`.\n" +
                 "Example 2: `filter -a penguins` will detect `I hate penguins` but not `I hate penguins2`.\n\n" +
                 "Specify an optional `punishment num` to customize the punishment for the filtered word. If " +
                 "unspecified, the default punishment is a message deletion.\n\n" +
                 "__Punishment types:__\n" +
                 "`Delete` - Delete the message\n" +
                 "`Mute` - Mute user (indefinitely)\n" + // TODO: Maybe allow servers to customize this duration.
                 "`Kick` - Kick the user\n" +
                 "`Ban` - Ban the user permanently\n" +
                 "`Shadowban` - Shadowban the user indefinitely")]
        [Remarks("[punishment] <word>")]
        public async Task CommandAddToFilter(string punishment, [Remainder] string word)
        {
            if (!Enum.TryParse(punishment, true, out FilterReactionEnum reaction))
            {
                await CommandAddToFilter(new FilteredWord
                {
                    ServerId = Context.Guild.Id,
                    Word = punishment + " " + word,
                    FilterReaction = FilterReactionEnum.Delete
                });
                
                return;
            }

            var fw = new FilteredWord
            {
                ServerId = Context.Guild.Id,
                Word = word,
                FilterReaction = reaction
            };

            await CommandAddToFilter(fw);
        }

        // This is left here to account for one-word filtered words. Ref: line 103 takes in 2+ args.
        [Priority(0)]
        [Command("-a", RunMode = RunMode.Async)]
        public async Task CommandAddToFilter([Remainder] string word)
        {
            var fw = new FilteredWord
            {
                ServerId = Context.Guild.Id,
                Word = word,
                FilterReaction = FilterReactionEnum.Delete
            };

            await CommandAddToFilter(fw);
        }

        private async Task CommandAddToFilter(FilteredWord fw)
        {
            if (!await _fwRepo.InsertIfNotExistsAsync(fw))
            {
                await SendBasicErrorEmbedAsync("This word already exists in your filter.");

                return;
            }

            await Context.Message.DeleteAsync();
            await SendBasicSuccessEmbedAsync("Successfully added the word to the filter.");
        }

        [Command("-r", RunMode = RunMode.Async)]
        [Summary("Removes a word or phrase from the word filter. If the word is a wildcard, you " +
                 "must specify so with the `*` indicator at the end of the word. Use this command " +
                 "with no arguments (filter) to view what's inside of the word filter.\n\n" +
                 "")]
        [Remarks("<word>")]
        public async Task CommandRemoveFromFilter([Remainder] string word)
        {
            FilteredWord fw = await _fwRepo.GetAsync(Context.Guild.Id, word);
            if (!await _fwRepo.DeleteIfExistsAsync(fw))
            {
                await SendBasicErrorEmbedAsync("The word you specified doesn't exist in the word filter.");

                return;
            }

            await SendBasicSuccessEmbedAsync("Successfully deleted the word from the filter.");
        }

        [Command("-c", RunMode = RunMode.Async)]
        [Summary("Clears the entire list of filtered phrases for the current server.")]
        public async Task CommandClearFilter()
        {
            FilteredWord[] curFilters = await _fwRepo.GetAllForServerAsync(Context.Guild.Id, true);
            if (!curFilters.Any())
            {
                await SendBasicErrorEmbedAsync("There are currently no filtered words.");

                return;
            }

            await _fwRepo.DeleteAllForServerAsync(Context.Guild.Id);
            await SendBasicSuccessEmbedAsync($"Successfully cleared the word filter for {Context.Guild.Name}.");
        }
    }
}