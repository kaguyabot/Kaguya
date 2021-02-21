using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Interactivity;
using Interactivity.Confirmation;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Overrides.Extensions;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands.Configuration
{
    [Module(CommandModule.Configuration)]
    [Group("greeting")]
    [Alias("greet")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireBotPermission(GuildPermission.SendMessages)]
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
                 "Optional paramaterized options may also be specified in your greeting message. " +
                 "Paramaters must be in all caps, otherwise they will not work.\n\n" +
                 "__**Message Parameters:**__\n" +
                 "- `{USERMENTION}` -> Mentions the user\n" +
                 "- `{MEMBERCOUNT}` -> The count of members in the server, formatted as `1st`, `2nd`, `3rd`, `4th`\n" +
                 "- `{SERVERNAME}` -> The name of the server")]
        [Remarks("<message>")]
        [Example("Welcome {USERMENTION} to {SERVERNAME}, you are the {MEMBERCOUNT} member!", ExampleStringFormat.CodeblockMultiLine)]
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

                await _serverRepository.UpdateAsync(server);
            }
        }
        
        [Command("-setchannel", RunMode = RunMode.Async)]
        [Summary("Sets the channel for where greetings will be sent.")]
        [Remarks("<message>")] // Delete if no remarks needed.
        public async Task GreetingSetChannelCommandAsync(ITextChannel textChannel)
        {
            var server = await _serverRepository.GetOrCreateAsync(Context.Guild.Id);

            ulong? oldChannel = server.CustomGreetingTextChannelId;
            bool hadPrevious = oldChannel.HasValue;
            
            server.CustomGreetingTextChannelId = textChannel.Id;
            await _serverRepository.UpdateAsync(server);

            string response = $"Linked greetings to {textChannel.Mention}";

            if (hadPrevious && oldChannel.Value != Context.Channel.Id)
            {
                var oldTextChannel = Context.Guild.GetTextChannel(oldChannel.Value);

                if (oldTextChannel != null)
                {
                    response += $" (was {oldTextChannel.Mention})";
                }
            }
            
            await SendBasicSuccessEmbedAsync(response);

            if (!server.CustomGreetingIsEnabled)
            {
                var toggleEmbed = GetBasicEmbedBuilder("I see that your greetings are disabled.\n" +
                                          "Would you like to turn them on?".AsBold(), KaguyaColors.LightYellow);

                var result = await _interactivityService.SendConfirmationAsync(toggleEmbed, Context.Channel, TimeSpan.FromMinutes(5));

                if (result.IsSuccess)
                {
                    server.CustomGreetingIsEnabled = true;
                    await _serverRepository.UpdateAsync(server);
                    
                    await SendBasicSuccessEmbedAsync("Greeting messages enabled. They will be sent in " +
                                                     $"{textChannel.Mention} when new members join the server.");
                }
                else if (result.IsCancelled)
                {
                    await SendBasicSuccessEmbedAsync("Greetings will remain disabled. Enable them with the " +
                                                     $"`{server.CommandPrefix}greeting -toggle` command.");
                }
            }
        }
        
        [Command("-clearmsg")]
        [Alias("-clear")]
        [Summary("Clears and disables the custom greeting, if one is set.")]
        [Remarks("<message>")] // Delete if no remarks needed.
        public async Task GreetingClearMessageCommandAsync()
        {
            var server = await _serverRepository.GetOrCreateAsync(Context.Guild.Id);

            server.CustomGreeting = null;
            server.CustomGreetingIsEnabled = false;

            await _serverRepository.UpdateAsync(server);
            
            await SendBasicSuccessEmbedAsync("The custom greeting message has been cleared. Greetings " +
                                           "have been disabled.");
        }
        
        [Command("-toggle")]
        [Summary("Toggles the sending of greeting messages on or off.")]
        [Remarks("<message>")] // Delete if no remarks needed.
        public async Task GreetingToggleCommandAsync()
        {
            var server = await _serverRepository.GetOrCreateAsync(Context.Guild.Id);
            server.CustomGreetingIsEnabled = !server.CustomGreetingIsEnabled;

            bool isNowEnabled = server.CustomGreetingIsEnabled;
            
            await _serverRepository.UpdateAsync(server);

            string state = isNowEnabled ? "enabled" : "disabled";
            await SendBasicSuccessEmbedAsync($"Successfully {state} greetings.\n" +
                                             $"Use `{server.CommandPrefix}greeting -view` to view " +
                                             $"the current message.");

            if (isNowEnabled && String.IsNullOrWhiteSpace(server.CustomGreeting))
            {
                await SendBasicEmbedAsync("It looks like your greetings are enabled " +
                                          "but you don't have a greeting message set.\n\n" +
                                          "In order for greetings to work, you must set the " +
                                          $"message via `{server.CommandPrefix}greeting -setmsg`.", KaguyaColors.LightYellow);
            }
        }
        
        [Command("-view")]
        [Summary("Displays the current greeting, if one is set.")]
        [Remarks("<message>")] // Delete if no remarks needed.
        public async Task GreetingViewCommandAsync()
        {
            var server = await _serverRepository.GetOrCreateAsync(Context.Guild.Id);
            var msg = server.CustomGreeting;

            if (String.IsNullOrWhiteSpace(msg))
            {
                await SendBasicErrorEmbedAsync("You haven't set up a message. Create one with " +
                                               $"`{server.CommandPrefix}greeting -setmsg`.");

                return;
            }

            await SendBasicEmbedAsync($"Current Greeting:".AsBoldUnderlined() + $"\n```{msg}```", KaguyaColors.Tan, false);
        }
    }
}