using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands.Configuration
{
    [Module(CommandModule.Configuration)]
    [Group("prefix")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class Prefix : KaguyaBase<Prefix>
    {
        private readonly KaguyaServerRepository _kaguyaServerRepository;
        private readonly ILogger<Prefix> _logger;

        public Prefix(ILogger<Prefix> logger, KaguyaServerRepository kaguyaServerRepository) : base(logger)
        {
            _logger = logger;
            _kaguyaServerRepository = kaguyaServerRepository;
        }

        [Command] // TODO: Hyperlink 'command prefix' to a doc of some sort.
        [Summary("Changes the command prefix for this server. Maximum length is 5 characters, and may not contain spaces. " +
                 "Remember, prefixes are case sensitive phrases.")]
        [Remarks("<prefix>")]
        public async Task PrefixCommand(string prefix)
        {
            KaguyaServer server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
            string curPrefix = server.CommandPrefix;

            if (curPrefix.Equals(prefix))
            {
                await SendBasicErrorEmbedAsync("The prefix you provided is already this server's command prefix.");

                return;
            }

            if (prefix.Contains(" "))
            {
                await SendBasicErrorEmbedAsync("Prefixes may not contain spaces.");

                return;
            }

            if (prefix.Length > 5)
            {
                await SendBasicErrorEmbedAsync("The prefix you provided is longer than the maximum length of 5 characters.");

                return;
            }

            if (string.IsNullOrWhiteSpace(prefix))
            {
                await SendBasicErrorEmbedAsync("The prefix you provided is invalid. Please try again with a new prefix.");

                return;
            }

            server.CommandPrefix = prefix;
            EmbedBuilder embed = GetBasicSuccessEmbedBuilder($"Successfully changed this server's command prefix to " + prefix.AsCodeBlockSingleLine(), true)
                .WithFooter(new EmbedFooterBuilder
                {
                    Text = "If you forget the prefix, " +
                           "execute \"@Kaguya prefix -v\" to view the current prefix."
                });

            await SendEmbedAsync(embed);
        }

        [Priority(1)]
        [Command("-v")]
        [Summary("Displays the current command prefix for this server.")]
        public async Task ViewPrefixCommand()
        {
            var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
            var commandPrefix = server.CommandPrefix;

            if (string.IsNullOrWhiteSpace(commandPrefix))
            {
                await SendBasicErrorEmbedAsync("The command prefix for this server is empty or is set to white-space only characters. " +
                                               "Please reset the prefix using the " + "prefix".AsCodeBlockSingleLine() + " command.");

                return;
            }
            
            await SendBasicEmbedAsync("The current prefix for this server is " + commandPrefix.AsCodeBlockSingleLine(), KaguyaColors.Blue);
        }
    }
}