using System;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord.WebSocket;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Internal.Enums;

namespace Kaguya.Discord.Commands.OwnerOnly
{
    [Restriction(ModuleRestriction.OwnerOnly)]
    [Module(CommandModule.OwnerOnly)]
    [Group("reawaken")]
    public class Reawaken : KaguyaBase<Reawaken>
    {
        private readonly ILogger<Reawaken> _logger;
        private readonly DiscordShardedClient _client;
        
        public Reawaken(ILogger<Reawaken> logger, DiscordShardedClient client) : base(logger)
        {
            _logger = logger;
            _client = client;
        }

        [Command(RunMode = RunMode.Async)]
        [Summary("Notification to all servers about the reawakening of Kaguya.")]
        public async Task ReawakenCommandAsync()
        {
            var awakenEmbed = new KaguyaEmbedBuilder(KaguyaColors.LightYellow)
            {
                Title = "Important Announcement: Version 4 Release",
                Description = "Important announcement from the Kaguya Administration:\n\n".AsBold() +
                              "Hello everyone! Long time no see. You may (or may not) have noticed " +
                              "Kaguya laying dormant in your server for the past 3 months, but she has returned!\n\n" +
                              "Kaguya has been updated to an all new version, rewritten from scratch. Kaguya " +
                              "is as fast and reliable as ever.\n\n" +
                              $"If you wish to continue using Kaguya, please take time to review the " +
                              $"changes [here]({Global.WikiV4ChangelogUrl}), there are a lot of them!\n" +
                              $"- [New Tutorial Video]({Global.VideoTutorialUrl})\n" +
                              $"- [Support Discord]({Global.SupportDiscordUrl})\n\n" +
                              $"We hope you enjoy the new look and functionality of Kaguya. Have a great day :)"
            }.WithFooter("-Kaguya Administration");

            int count = 1;
            int max = _client.Guilds.Count;
            
            foreach (var guild in _client.Guilds)
            {
                try
                {
                    await guild.DefaultChannel.SendMessageAsync(embed: awakenEmbed.Build());
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, $"Failure: {count} / {max} guilds processed as part of reawaken operation.");
                }
                
                _logger.LogInformation($"Success: {count} / {max} guilds processed as part of reawaken operation.");

                count++;
            }
        }
    }
}