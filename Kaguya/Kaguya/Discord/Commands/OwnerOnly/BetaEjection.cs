using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Interactivity;
using Interactivity.Confirmation;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands.OwnerOnly
{
    [Restriction(ModuleRestriction.OwnerOnly)]
    [Module(CommandModule.OwnerOnly)]
    [Group("betaejection")]
    public class BetaEjection : KaguyaBase<BetaEjection>
    {
        private readonly ILogger<BetaEjection> _logger;
        private readonly DiscordShardedClient _client;
        private readonly InteractivityService _interactivityService;
        
        private const ulong BETA_ID = 367403886841036820;
        
        public BetaEjection(ILogger<BetaEjection> logger, DiscordShardedClient client,
            InteractivityService interactivityService) : base(logger)
        {
            _logger = logger;
            _client = client;
            _interactivityService = interactivityService;
        }

        [Command(RunMode = RunMode.Async)]
        [Summary("Ejects the bot from all servers it is connected to. ONLY EXECUTE ON BETA.")]
        public async Task BetaEjectionCommandAsync()
        {
            if (_client.CurrentUser.Id != BETA_ID)
            {
                await SendBasicErrorEmbedAsync("Beta id mismatch. Aborting.");

                return;
            }

            var confrimationEmbed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
                .WithDescription("Are you sure you want to eject the beta bot from " 
                                 + _client.Guilds.Count + " guilds?");

            Confirmation confirmation = new ConfirmationBuilder()
                                               .WithContent(PageBuilder.FromEmbed(confrimationEmbed.Build()))
                                               .Build();

            var result = await _interactivityService.SendConfirmationAsync(confirmation, Context.Channel);

            if (result.IsSuccess)
            {
                var ejectionEmbed = new KaguyaEmbedBuilder(KaguyaColors.Orange)
                {
                    Title = "Announcement: End of Beta",
                    Description = "Important announcement from the Kaguya Administration:\n\n".AsBold() +
                                  "Thank you all so much for participating in the open beta! " +
                                  "We have learned a lot from your feedback and have tried to make " +
                                  "Kaguya v4 an amazing option for every Discord server.\n\n" +
                                  "If you want to continue using Kaguya, please invite the official version " +
                                  $"[HERE]({Global.InviteUrl}). This live version will be up very soon, " +
                                  $"all you have to do is add it to this server and wait for the reboot " +
                                  $"announcement!\n\n" +
                                  $"**The beta is now closed. I have been instructed to auto-eject from this server. " +
                                  $"Goodbye!**"
                };
                
                foreach (var guild in _client.Guilds)
                {
                    try
                    {
                        await guild.DefaultChannel.SendMessageAsync(embed: ejectionEmbed.Build());
                        _logger.LogInformation($"Disconnected from guild {guild.Id} as part of the ejection operation.");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Failed to send ejection embed to default channel in guild {guild.Id}.");
                    }
                    
                    await guild.LeaveAsync();
                }
            }
            else
            {
                await SendBasicErrorEmbedAsync("Operation aborted.");

                return;
            }

            await SendBasicSuccessEmbedAsync("Operation complete.");
        }
    }
}