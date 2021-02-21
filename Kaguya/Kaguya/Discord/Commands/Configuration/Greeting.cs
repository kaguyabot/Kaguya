using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Interactivity;
using Interactivity.Confirmation;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands.Configuration
{
    [Module(CommandModule.Configuration)]
    [Group("greeting")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireUserPermission(GuildPermission.SendMessages)]
    public class Greeting : KaguyaBase<Greeting>
    {
        private readonly ILogger<Greeting> _logger;
        private readonly KaguyaServerRepository _serverRepository;
        private readonly InteractivityService _interactivityService;

        public Greeting(ILogger<Greeting> logger, KaguyaServerRepository serverRepository,
            InteractivityService interactivityService) : base(logger)
        {
            _logger = logger;
            _serverRepository = serverRepository;
            _interactivityService = interactivityService;
        }

        [Command("-setmsg", RunMode = RunMode.Async)]
        [Summary("Allows you to set the custom greeting message for your server. This message " +
                 "will be displayed to new people when they join your server for the first time. " +
                 "Optional paramaterized options may also be specified in your greeting message.\n\n" +
                 "__**Message Parameters:**__\n" +
                 "- `{USERMENTION}` -> Mentions the user\n" +
                 "- `{MEMBERCOUNT}` -> The count of members in your server, formatted as `1st`, `2nd`, `3rd`, `4th`\n")]
        [Remarks("<message>")]
        public async Task GreetingSetMessageCommandAsync([Remainder] string message)
        {
            if (message.Length > 1750)
            {
                await SendBasicErrorEmbedAsync("Sorry, your message needs to contain no more than " +
                                               "1,750 characters.");

                return;
            }
            var server = await _serverRepository.GetOrCreateAsync(Context.Guild.Id);

            server.CustomGreeting = message;
            var firstEmbed = new KaguyaEmbedBuilder(KaguyaColors.ConfigurationColor)
            {
                Description = "Successfully Set Custom Greeting:".AsBold() +
                              "\n\n" +
                              message.AsCodeBlockMultiline()
            };

            await SendEmbedAsync(firstEmbed);
            
            if(!server.CustomGreetingTextChannelId.HasValue || 
               !Context.Guild.TextChannels.Any(x => x.Id == server.CustomGreetingTextChannelId.Value))
            {
                var secondEmbed = new KaguyaEmbedBuilder(KaguyaColors.ConfigurationColor)
                {
                    Description = $"{Context.User.Mention} I see that there isn't a text channel for " +
                                  $"greetings to be sent in.\n\n" +
                                  $"Would you like to set a greeting channel?".AsBold()
                };

                var confirmation = new ConfirmationBuilder()
                                   .WithContent(PageBuilder.FromEmbedBuilder(secondEmbed))
                                   .WithDeletion(DeletionOptions.AfterCapturedContext | DeletionOptions.Invalids)
                                   .Build();

                var response = await _interactivityService.SendConfirmationAsync(confirmation, Context.Channel, TimeSpan.FromMinutes(5));

                if (response.IsSuccess)
                {
                    await SendEmbedAsync(GetBasicEmbedBuilder("Where do you want to send your greetings?\n" +
                                                              "Mention a text channel. Example: `#my-channel`", KaguyaColors.ConfigurationColor));
                    
                    var channelReponse = await _interactivityService
                        .NextMessageAsync(x => x.MentionedChannels.Any(), 
                            timeout: TimeSpan.FromMinutes(5));

                    if (channelReponse.IsTimeouted)
                    {
                        await SendBasicErrorEmbedAsync("Greeting channel response timed out. Aborting setup.");

                        return;
                    }

                    var channel = channelReponse.Value.MentionedChannels.FirstOrDefault() as ITextChannel;
                    if (channel == null)
                    {
                        await SendBasicErrorEmbedAsync("An error occurred while trying to find your " +
                                                       "text channel. Please ensure you are " +
                                                       "mentioning a proper text channel. If the issue persists, please " +
                                                       "report this issue to the developers.");
                        return;
                    }

                    server.CustomGreetingTextChannelId = channel.Id;

                    await SendBasicSuccessEmbedAsync($"I've linked new greetings to {channel.Mention}");
                }
                else if(response.IsCancelled)
                {
                    await SendBasicSuccessEmbedAsync($"Okay, no channel will be set. Use the \n" +
                                                     $"`{server.CommandPrefix}greeting -setchannel` " +
                                                     $"command to do this step later.");
                }
                else if (response.IsTimeouted)
                {
                    var embed = GetBasicErrorEmbedBuilder("Response timed out. No action will be taken.");

                    _interactivityService.DelayedSendMessageAndDeleteAsync(Context.Channel, 
                        deleteDelay: TimeSpan.FromSeconds(15), embed: embed.Build());
                }
            }
        }
        
        [Command("-setchannel")]
        [Summary("Sets the channel for where greetings will be sent.")]
        [Remarks("<message>")] // Delete if no remarks needed.
        public async Task GreetingSetChannelCommandAsync(ITextChannel textChannel)
        {
            
        }
        
        [Command("-clearmsg")]
        [Alias("-clear")]
        [Summary("Clears and disables the custom greeting, if one is set.")]
        [Remarks("<message>")] // Delete if no remarks needed.
        public async Task GreetingClearMessageCommandAsync()
        {
            
        }
        
        [Command("-toggle")]
        [Summary("Toggles the sending of greeting messages on or off.")]
        [Remarks("<message>")] // Delete if no remarks needed.
        public async Task GreetingToggleCommandAsync()
        {
            
        }
        
        [Command("-view")]
        [Summary("Displays the current greeting, if one is set.")]
        [Remarks("<message>")] // Delete if no remarks needed.
        public async Task GreetingViewCommandAsync()
        {
            
        }
    }
}