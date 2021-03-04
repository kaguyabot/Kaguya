using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Humanizer;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;

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
        private readonly KaguyaServerRepository _kaguyaServerRepository;
        private readonly IList<PropertyInfo> _logProperties;
        
        public Log(ILogger<Log> logger, LogConfigurationRepository logConfigurationRepository,
            KaguyaServerRepository kaguyaServerRepository) : base(logger)
        {
            _logger = logger;
            _logConfigurationRepository = logConfigurationRepository;
            _kaguyaServerRepository = kaguyaServerRepository;
            _logProperties = LogConfiguration.LogProperties;
            if (_logProperties == null)
            {
                LogConfiguration.LoadProperties();
                _logProperties = LogConfiguration.LogProperties;
            }
        }

        [Command]
        [Summary("Displays all log types and their assignments for this server.")]
        public async Task LogViewCommand()
        {
            var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
            var logConfig = await _logConfigurationRepository.GetOrCreateAsync(Context.Guild.Id);
            var sb = new StringBuilder();

            bool updateDb = false;
            foreach (var prop in _logProperties.OrderBy(x => x.Name))
            {
                ulong channelId;

                try
                {
                    channelId = (ulong) prop.GetValue(logConfig, null); // ID of the channel the log is assigned to.
                }
                catch (NullReferenceException)
                {
                    channelId = 0;
                }
                    
                ITextChannel channel = Context.Guild.GetTextChannel(channelId);

                if (channel == null && channelId != 0)
                {
                    updateDb = true;
                    prop.SetValue(logConfig, (ulong) 0);
                }
            
                sb.AppendLine($"{prop.Name} - {channel?.Mention ?? "Not Assigned".AsBold()}");
            }

            if (updateDb)
            {
                await _logConfigurationRepository.UpdateAsync(logConfig);
            }

            string sPrefix = server.CommandPrefix;
            var embed = new KaguyaEmbedBuilder(KaguyaColors.ConfigurationColor)
            {
                Title = $"Log Configuration for {Context.Guild.Name}",
                Description = sb.ToString()
            }.WithFooter($"Use the {sPrefix}log -set all command to set all log types to one channel.\n" +
                         $"Use the {sPrefix}log -reset command to disable logging for some or all log types.");

            await SendEmbedAsync(embed);
        }
        
        [Command("-set")]
        [Summary("Sets a logtype to a text channel. If no text channel is provided, the " +
                 "channel the command is invoked from is selected. All logs for the logtype will be " +
                 "sent to the specified channel. You may also use the 'all' logtype to set " +
                 "everything to the same channel simultaneously.")]
        [Remarks("<log type> [text channel]")]
        public async Task LogSetCommand(string logType, SocketTextChannel textChannel = null)
        {
            await ModifyLogConfigAsync(logType, textChannel, false);
        }
        
        [Command("-reset")]
        [Summary("Resets a logtype back to it's default state (disabled). Pass in \"all\" as the log type " +
                 "to reset all log types.")]
        [Remarks("<log type>")]
        public async Task LogSetCommand(string logType)
        {
            await ModifyLogConfigAsync(logType, null, true);
        }

        private async Task ModifyLogConfigAsync(string logType, SocketTextChannel textChannel, bool reset)
        {
            if (textChannel == null && !reset)
            {
                textChannel = Context.Channel as SocketTextChannel;

                if (textChannel == null)
                {
                    await SendBasicErrorEmbedAsync("Invalid text channel. Channel could not be found or does not exist.");

                    return;
                }
            }
            
            IList<PropertyInfo> properties = _logProperties;
            LogConfiguration logConfig = await _logConfigurationRepository.GetOrCreateAsync(Context.Guild.Id);

            bool all = false;
            
            if (logType.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                all = true;
                MassModifyConfig(ref logConfig, reset ? 0 : textChannel?.Id ?? 0);
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
                
                propMatch.SetValue(logConfig, reset ? 0 : textChannel.Id);
                
                await _logConfigurationRepository.UpdateAsync(logConfig);

                if (reset)
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
                    prop.SetValue(logConfig, reset ? 0 : textChannel.Id);
                }
                
                await _logConfigurationRepository.UpdateAsync(logConfig);

                if (reset)
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