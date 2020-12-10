using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Discord.Commands;
using Kaguya.Discord.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Humanizer;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.DiscordExtensions;

namespace Kaguya.Discord.Commands.Configuration
{
    [Module(CommandModule.Configuration)]
    [Group("log")]
    [Alias("l")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class Log : KaguyaBase<Log>
    {
        private readonly ILogger<Log> _logger;
        private readonly LogConfigurationRepository _logConfigurationRepository;
        
        public Log(ILogger<Log> logger, LogConfigurationRepository logConfigurationRepository) : base(logger)
        {
            _logger = logger;
            _logConfigurationRepository = logConfigurationRepository;
        }

        [Command]
        [Summary("Displays all logtypes and their assignments for this server.")]
        public async Task LogViewCommand()
        {
            var logConfig = _logConfigurationRepository.GetOrCreateAsync(Context.Guild.Id);
            
        }
        
        [Command("-set")]
        [Summary("Sets a logtype to a text channel. All logs for the logtype will be " +
                 "sent to the specified channel.")]
        [Remarks("<log type> <text channel>")]
        public async Task LogSetCommand(string logType, SocketTextChannel textChannel)
        {
            await ModifyLogtype(logType, textChannel);
        }
        
        // TODO: -unset command where we again use reflection to set a log channel back to 0 id.
        [Command("-reset")]
        [Summary("Resets a logtype back to it's default state (disabled).")]
        [Remarks("<log type>")]
        public async Task LogSetCommand(string logType)
        {
            await ModifyLogtype(logType, null);
        }

        private async Task ModifyLogtype(string logType, SocketTextChannel? textChannel)
        {
            List<PropertyInfo> properties = GetLogProperties().ToList();

            if (!properties.Any())
            {
                await SendBasicErrorEmbedAsync("An unexpected error occurred. Properties for type LogConfiguration could not be found. " +
                                               "Please contact the developer.");

                return;
            }

            string[] validLogtypes = properties.Select(x => x.Name).ToArray();
            string exactMatch = validLogtypes.FirstOrDefault(x => x.Equals(logType, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(exactMatch))
            {
                string noMatch = $"No log type found for: {logType.AsBold()}.";
                var finalSb = new StringBuilder(noMatch);

                var likelyMatches = LikelyMatches(logType, validLogtypes);
                if (likelyMatches.Any())
                {
                    finalSb.Append($" Did you mean:\n\n{likelyMatches.Humanize(x => x.AsBold()).Replace("and", "or")}?");
                }

                await SendBasicErrorEmbedAsync(finalSb.ToString());

                return;
            }

            LogConfiguration server = await _logConfigurationRepository.GetOrCreateAsync(Context.Guild.Id);

            var propMatch = properties.FirstOrDefault(x => x.Name.Equals(exactMatch, StringComparison.OrdinalIgnoreCase));
            if (propMatch == null)
            {
                await SendBasicErrorEmbedAsync("An unexpected error occurred. Property could not be found for exact match. " +
                                               "Please contact the developer.");

                return;
            }
            propMatch.SetValue(server, textChannel?.Id ?? 0);

            await _logConfigurationRepository.UpdateAsync(server);

            if (textChannel == null)
            {
                await SendBasicSuccessEmbedAsync($"Reset logtype {propMatch.Name.AsBold()}.");
            }
            else
            {
                await SendBasicSuccessEmbedAsync($"Set logtype {propMatch.Name.AsBold()} to {textChannel.Name.AsCodeBlockSingleLine().AsBold()}");
            }
        }

        private static IList<PropertyInfo> GetLogProperties()
        {
            IList<PropertyInfo> properties = typeof(LogConfiguration).GetProperties()
                                                                           .Where(x => !x.Name.Equals("ServerId", StringComparison.OrdinalIgnoreCase))
                                                                           .ToList();

            return properties;
        }

        private string[] LikelyMatches(string s, string[] toCheck)
        {
           return toCheck.Where(x => x.Contains(s, StringComparison.OrdinalIgnoreCase)).ToArray();
        }
    }
}