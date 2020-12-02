using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Attributes;
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
        private readonly ILogger<Filter> _logger;
        private readonly WordFilterRepository _fwRepo;

        protected Filter(ILogger<Filter> logger, WordFilterRepository fwRepo) : base(logger)
        {
            _logger = logger;
            _fwRepo = fwRepo;
        }

        [Command]
        [Summary("Displays the currently filtered words.")]
        public async Task CommandViewFilter()
        {
            // TODO: Convert to pagination
        }

        [Priority(1)]
        [Command("-a")]
        [Summary("Adds a word to the filter. Append a `*` character at the end to have the word " +
                 "be marked as a wildcard. Wildcards are detected if any portion of a user's message " +
                 "contains the filtered keyword. A non-wildcard filtered word will only be filtered if " +
                 "any word in the user's message is the word in the filter.\n\n" +
                 "Example: `filter -a tacos*` will detect `tacos`, `tacosopweijfgwoeifj`, `tacos+burritos`.\n" +
                 "Example 2: `filter -a penguins` will detect `I hate penguins` but not `I hate penguins2`.\n\n" +
                 "Specify an optional `punishment num` to customize the punishment for the filtered word. If " +
                 "unspecified, the default punishment is a message deletion.\n\n" +
                 "__Punishment nums:__\n" +
                 "`0` - Delete the message\n" +
                 "`1` - Mute user (indefinitely)\n" + // TODO: Maybe allow servers to customize this duration.
                 "`2` - Kick the user\n" +
                 "`3` - Ban the user permanently\n" +
                 "`4` - Shadowban the user indefinitely")]
        [Remarks("[punishment num] <word>")]
        public async Task CommandAddToFilter(int reactionNum, [Remainder] string word)
        {
            var fw = new FilteredWord
            {
                ServerId = Context.Guild.Id,
                Word = word,
                FilterReaction = (FilterReactionEnum)reactionNum
            };

            await CommandAddToFilter(fw);
        }
        
        [Priority(0)]
        [Command("-a")]
        public async Task CommandAddToFilter([Remainder]string word)
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

        [Command("-r")]
        [Summary("Removes a word or phrase from the word filter. If the word is a wildcard, you " +
                 "must specify so with the `*` indicator at the end of the word. Use this command " +
                 "with no arguments (filter) to view what's inside of the word filter.\n\n" +
                 "")]
        public async Task CommandRemoveFromFilter([Remainder] string word)
        {
            var fw = await _fwRepo.GetAsync(Context.Guild.Id, word);
            if (!await _fwRepo.DeleteIfExistsAsync(fw))
            {
                await SendBasicErrorEmbedAsync("The word you specified doesn't exist in the word filter.");

                return;
            }

            await SendBasicSuccessEmbedAsync("Successfully deleted the word from the filter.");
        }

        [Command("-c")]
        [Summary("Clears the entire list of filtered phrases for the current server.")]
        public async Task CommandClearFilter()
        {
            var curFilters = await _fwRepo.GetAllForServerAsync(Context.Guild.Id, true);
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