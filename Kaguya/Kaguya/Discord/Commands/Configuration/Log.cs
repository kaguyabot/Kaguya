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
using Kaguya.Discord.Attributes.Enums;
using Kaguya.Discord.DiscordExtensions;

namespace Kaguya.Discord.Commands.Configuration
{
    [Module(CommandModule.Configuration)]
    [Group("log")]
    [Alias("l", "logs")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class Log : KaguyaBase<Log>
    {
        private readonly ILogger<Log> _logger;
        private readonly LogConfigurationRepository _logConfigurationRepository;
        private readonly IList<PropertyInfo> _logProperties = LogConfiguration.LogProperties ?? LogConfiguration.GetLogProperties();
        
        public Log(ILogger<Log> logger, LogConfigurationRepository logConfigurationRepository) : base(logger)
        {
            _logger = logger;
            _logConfigurationRepository = logConfigurationRepository;
        }

        [Command]
        [Summary("Displays all log types and their assignments for this server.")]
        public async Task LogViewCommand()
        {
            var logConfig = await _logConfigurationRepository.GetOrCreateAsync(Context.Guild.Id);
            var sb = new StringBuilder();

            bool updateDb = false;
            foreach (var prop in _logProperties.OrderBy(x => x.Name))
            {
                object channelId = prop.GetValue(logConfig, null); // ID of the channel the log is assigned to.

                if (channelId is not ulong id) // This also checks for null
                {
                    continue;
                }

                ITextChannel channel = Context.Guild.GetTextChannel(id);

                if (channel == null && id != 0)
                {
                    updateDb = true;
                    prop.SetValue(logConfig, (ulong)0);
                    
                    continue;
                }
                
                sb.AppendLine($"{prop.Name} - {channel?.Mention ?? "Not Assigned".AsBold()}");
            }

            if (updateDb)
            {
                await _logConfigurationRepository.UpdateAsync(logConfig);
            }

            var embed = new KaguyaEmbedBuilder(KaguyaColors.Green)
            {
                Title = $"Log Configuration for {Context.Guild.Name}",
                Description = sb.ToString()
            };

            await SendEmbedAsync(embed);
        }
        
        [Command("-set")]
        [Summary("Sets a logtype to a text channel. All logs for the logtype will be " +
                 "sent to the specified channel. You may also use the 'all' logtype to set " +
                 "everything to the same channel simultaneously.")]
        [Remarks("<log type> <text channel>")]
        public async Task LogSetCommand(string logType, SocketTextChannel textChannel)
        {
            await ModifyLogConfigAsync(logType, textChannel);
        }
        
        [Command("-reset")]
        [Summary("Resets a logtype back to it's default state (disabled). Pass in \"all\" as the log type " +
                 "to reset all log types.")]
        [Remarks("<log type>")]
        public async Task LogSetCommand(string logType)
        {
            await ModifyLogConfigAsync(logType, null);
        }

        private async Task ModifyLogConfigAsync(string logType, SocketTextChannel textChannel)
        {
            IList<PropertyInfo> properties = _logProperties;
            LogConfiguration logConfig = await _logConfigurationRepository.GetOrCreateAsync(Context.Guild.Id);

            bool all = false;
            
            if (logType.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                all = true;
                MassModifyConfig(ref logConfig, textChannel?.Id ?? 0);
            }

            if (!properties.Any())
            {
                await SendBasicErrorEmbedAsync("An unexpected error occurred. Properties for type LogConfiguration could not be found. " +
                                               "Please contact the developer.");

                return;
            }

            if (!all)
            {
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

                var propMatch = properties.FirstOrDefault(x => x.Name.Equals(exactMatch, StringComparison.OrdinalIgnoreCase));
                if (propMatch == null)
                {
                    await SendBasicErrorEmbedAsync("An unexpected error occurred. Property could not be found for exact match. " +
                                                   "Please contact the developer.");

                    return;
                }
                
                propMatch.SetValue(logConfig, textChannel?.Id ?? 0);
                
                await _logConfigurationRepository.UpdateAsync(logConfig);

                if (textChannel == null)
                {
                    await SendBasicSuccessEmbedAsync($"Reset logtype {propMatch.Name.AsBold()}.");
                }
                else
                {
                    await SendBasicSuccessEmbedAsync($"Set logtype {propMatch.Name.AsBold()} to {textChannel.Name.AsCodeBlockSingleLine().AsBold()}");
                }
            }
            else
            {
                foreach (PropertyInfo prop in _logProperties)
                {
                    prop.SetValue(logConfig, textChannel?.Id ?? 0);
                }
                
                await _logConfigurationRepository.UpdateAsync(logConfig);

                if (textChannel == null)
                {
                    await SendBasicSuccessEmbedAsync("Reset all logtypes.");
                }
                else
                {
                    await SendBasicSuccessEmbedAsync($"Set all logtypes to {textChannel.Mention}");
                }
            }
        }

        /// <summary>
        /// Sets all loggable properties in the <see cref="config"/> to the <see cref="channelId"/>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="channelId">Must be 0 if resetting</param>
        private void MassModifyConfig(ref LogConfiguration config, ulong channelId)
        {
            foreach (var prop in _logProperties)
            {
                prop.SetValue(config, channelId);
            }
        }
        
        private string[] LikelyMatches(string s, string[] toCheck)
        {
           return toCheck.Where(x => x.Contains(s, StringComparison.OrdinalIgnoreCase)).ToArray();
        }
    }
}