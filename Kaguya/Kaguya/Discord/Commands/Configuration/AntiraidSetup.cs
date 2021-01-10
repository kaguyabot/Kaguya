using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Humanizer.Localisation;
using Interactivity;
using Interactivity.Confirmation;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Discord.Parsers;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands.Configuration
{
    [Module(CommandModule.Configuration)]
    [Group("antiraid")]    
    [Alias("ar")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.KickMembers)]
    [RequireBotPermission(GuildPermission.MuteMembers)]
    [RequireBotPermission(GuildPermission.DeafenMembers)]
    public class AntiraidSetup : KaguyaBase<AntiraidSetup>
    {
        private readonly ILogger<AntiraidSetup> _logger;
        private readonly InteractivityService _interactivityService;
        private readonly KaguyaServerRepository _kaguyaServerRepository;
        private readonly AntiraidConfigRepository _antiraidConfigRepository;
        private readonly CommonEmotes _commonEmotes;
        private static readonly ConcurrentDictionary<ulong, bool> _currentlyActiveSetups = new();

        public AntiraidSetup(ILogger<AntiraidSetup> logger, InteractivityService interactivityService,
            KaguyaServerRepository kaguyaServerRepository, AntiraidConfigRepository antiraidConfigRepository, 
            CommonEmotes commonEmotes) : base(logger)
        {
            _logger = logger;
            _interactivityService = interactivityService;
            _kaguyaServerRepository = kaguyaServerRepository;
            _antiraidConfigRepository = antiraidConfigRepository;
            _commonEmotes = commonEmotes;
        }

        [Command(RunMode = RunMode.Async)]
        [Summary("Displays an interactive setup utility for configuring the Kaguya Antiraid service.")]
        public async Task AntiraidSetupCommand()
        {
            /* todo: Ensure more than 1 setup cannot run in a given server at a time. */

            if (_currentlyActiveSetups.ContainsKey(Context.Guild.Id))
            {
                await SendBasicErrorEmbedAsync("There is already an active antiraid setup running in this server. Please complete the first " +
                                               "setup before beginning this one.");
                return;
            }

            _currentlyActiveSetups.GetOrAdd(Context.Guild.Id, true);
            
            var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
            var config = await _antiraidConfigRepository.GetAsync(Context.Guild.Id);

            if (config != null)
            {
                if (!await ConfirmConfigOverwriteAsync(config))
                {
                    Remove(server.ServerId);
                    return;
                }
            }

            await SendEmbedAsync(GetStageOneEmbed());

            var newArConfig = new AntiRaidConfig
            {
                ServerId = Context.Guild.Id
            };
            
            bool stageOneClear = false;
            bool stageTwoClear = false;
            bool stageThreeClear = false;

            int failureAttempts = 0;
            
            // Stage 1
            while (!stageOneClear)
            {
                var userMessage = await _interactivityService.NextMessageAsync(x => x.Author == Context.User, null, TimeSpan.FromMinutes(2));
                if (!await ValidateStageOneTwoInputAsync(userMessage))
                {
                    failureAttempts++;

                    if (await SendFailureEmbedAsync(failureAttempts))
                    {
                        Remove(server.ServerId);

                        return;
                    }
                    
                    continue;
                }

                uint userThreshold = uint.Parse(userMessage.Value.Content);
                newArConfig.UserThreshold = userThreshold;
                
                stageOneClear = true;
            }

            failureAttempts = 0;
            await SendEmbedAsync(GetStageTwoEmbed(newArConfig.UserThreshold));

            // Stage 2
            while (!stageTwoClear)
            {
                var userMessage = await _interactivityService.NextMessageAsync(x => x.Author == Context.User, null, TimeSpan.FromMinutes(2));
                
                if (!await ValidateStageOneTwoInputAsync(userMessage))
                {
                    failureAttempts++;

                    if (await SendFailureEmbedAsync(failureAttempts))
                    {
                        Remove(server.ServerId);

                        return;
                    }
                    
                    continue;
                }

                uint secondsThreshold = uint.Parse(userMessage.Value.Content);
                newArConfig.Seconds = secondsThreshold;
                
                stageTwoClear = true;
            }

            failureAttempts = 0;
            await SendEmbedAsync(GetStageThreeEmbed(newArConfig.UserThreshold, newArConfig.Seconds));

            // Stage 3
            while (!stageThreeClear)
            {
                var userMessage = await _interactivityService.NextMessageAsync(x => x.Author == Context.User, null, TimeSpan.FromMinutes(2));
                
                if (!ValidateStageThreeInput(userMessage.Value.Content))
                {
                    failureAttempts++;

                    await SendStageThreeFailureAsync(userMessage.Value.Content);
                    
                    if (await SendFailureEmbedAsync(failureAttempts))
                    {
                        Remove(server.ServerId);

                        return;
                    }
                    
                    continue;
                }

                newArConfig.Action = GetAntiraidActionEnum(userMessage.Value.Content);
                stageThreeClear = true;
            }

            // Stage 4
            failureAttempts = 0;
            if (newArConfig.Action == AntiraidAction.Mute || newArConfig.Action == AntiraidAction.Ban)
            {
                var confirmationBuilder = new ConfirmationBuilder()
                                          .WithConfirmEmote(_commonEmotes.CheckMarkEmoji)
                                          .WithDeclineEmote(_commonEmotes.RedCrossEmote)
                                          .WithUsers(Context.User)
                                          .WithContent(PageBuilder.FromEmbed(GetStageFourConfirmationEmbed(newArConfig.Action)))
                                          .Build();

                var confirmation = await _interactivityService.SendConfirmationAsync(confirmationBuilder, Context.Channel, TimeSpan.FromMinutes(2));
                
                if (confirmation.IsSuccess)
                {
                    // Stage 5
                    await SendEmbedAsync(GetStageFiveEmbed());
                    
                    var nextMessage = await _interactivityService.NextMessageAsync(x => x.Author == Context.User, null, TimeSpan.FromMinutes(2));
                    while (true)
                    {
                        if (!ValidateStageFiveInput(nextMessage.Value.Content))
                        {
                            failureAttempts++;
                            
                            if (await SendFailureEmbedAsync(failureAttempts))
                            {
                                Remove(server.ServerId);

                                return;
                            }
                            
                            await SendEmbedAsync(GetStageFiveErrorEmbed());
                        }
                        else
                        {
                            newArConfig.PunishmentLength = GetStageFiveTimeSpan(nextMessage.Value.Content);

                            break;
                        }
                    }
                }
                else
                {
                    await SendBasicEmbedAsync("Got it, users will be punished indefinitely.", KaguyaColors.IceBlue);
                }
            }

            // End
            Remove(server.ServerId);
            
            await _antiraidConfigRepository.InsertOrUpdateAsync(newArConfig);
            await SendEmbedAsync(GetFinalEmbed(newArConfig, server.CommandPrefix));
        }

        private async Task<bool> SendFailureEmbedAsync(int failureAttempts)
        {
            if (failureAttempts > 2)
            {
                await SendBasicErrorEmbedAsync("You have entered an incorrect response too many times. Please try again.");

                return true;
            }

            return false;
        }

        private async Task<bool> ConfirmConfigOverwriteAsync(AntiRaidConfig config)
        {
            if (config == null)
            {
                return true;
            }

            var pageBuilder = new PageBuilder()
                              .WithDescription($"{Context.User.Mention} I see that your server already has a defined anti-raid configuration:\n\n" +
                                               $"{GetAntiraidConfigString(config)}\n" +
                                               $"Are you sure you would like to change this config?")
                              .WithColor(KaguyaColors.IceBlue);

            var builder = new ConfirmationBuilder()
                          .WithContent(pageBuilder)
                          .WithUsers(Context.User)
                          .WithConfirmEmote(_commonEmotes.CheckMarkEmoji)
                          .WithDeclineEmote(_commonEmotes.RedCrossEmote)
                          .Build();

            var response = await _interactivityService.SendConfirmationAsync(builder, Context.Channel, TimeSpan.FromMinutes(2));

            if (!response.IsSuccess)
            {
                await SendBasicEmbedAsync("Okay, I will leave the config how it is.", KaguyaColors.IceBlue);

                return false;
            }

            return true;
        }

        private string GetAntiraidConfigString(AntiRaidConfig config)
        {
            var configStringBuilder = new StringBuilder();
            configStringBuilder.AppendLine($"Action: {config.Action.Humanize().AsBold()}");
            configStringBuilder.AppendLine($"User threshold: {config.UserThreshold.ToString("N0").AsBold()}");
            configStringBuilder.AppendLine($"Time threshold: {TimeSpan.FromSeconds(config.Seconds).Humanize(3, minUnit: TimeUnit.Second).AsBold()}");

            string userPunishDuration = "User punishment duration: ";
            
            if (config.Action == AntiraidAction.Ban || config.Action == AntiraidAction.Mute)
            {
                string durStr = "Indefinite";

                if (config.PunishmentLength.HasValue)
                {
                    durStr = config.PunishmentLength.Value.Humanize(2, minUnit: TimeUnit.Second);
                }

                userPunishDuration += durStr.AsBold();
            }
            else
            {
                userPunishDuration += "N/A";
            }

            configStringBuilder.AppendLine(userPunishDuration);

            return configStringBuilder.ToString();
        }

        private async Task<bool> ValidateStageOneTwoInputAsync(InteractivityResult<SocketMessage> nextMessage)
        {
            if (nextMessage.IsTimeouted)
            {
                return false;
            }

            string content = nextMessage.Value.Content;
            if (!uint.TryParse(content, out uint userThreshold))
            {
                await SendBasicErrorEmbedAsync("Hm, looks like this input is invalid. Please try again.");
                
                return false;
            }

            if (userThreshold < 2)
            {
                await SendBasicErrorEmbedAsync("Please enter a user value that is greater than 1. " +
                                               "A raid of 1 user would mean any user who joins your server would be actioned.".AsItalics() +
                                               "\n\nPlease try again.");
                    
                return false;
            }

            if (userThreshold > 500)
            {
                await SendBasicErrorEmbedAsync("The user threshold must be no greater than 500. Please try again with a new number.");
                    
                return false;
            }
                
            return true;
        }

        private bool ValidateStageThreeInput(string input)
        {
            return input.ToLower() switch
            {
                "mute" => true,
                "kick" => true,
                "ban" => true,
                "shadowban" => true,
                _ => false
            };
        }

        private bool ValidateStageFiveInput(string input)
        {
            var timeParser = new TimeParser(input);
            var parsedTime = timeParser.ParseTime();

            return parsedTime != TimeSpan.Zero && parsedTime >= TimeSpan.FromSeconds(5) && parsedTime <= TimeSpan.FromDays(365);
        }

        private TimeSpan GetStageFiveTimeSpan(string input) => new TimeParser(input).ParseTime();

        private AntiraidAction GetAntiraidActionEnum(string input)
        {
            return input.ToLower() switch
            {
                "mute" => AntiraidAction.Mute,
                "kick" => AntiraidAction.Kick,
                "ban" => AntiraidAction.Ban,
                "shadowban" => AntiraidAction.Shadowban,
                _ => throw new ArgumentException($"{input} could not be parsed into an AntiraidAction.")
            };
        }

        private async Task SendStageThreeFailureAsync(string wrongInput)
        {
            var embed = new KaguyaEmbedBuilder(KaguyaColors.LightYellow)
                .WithDescription($"The action `{wrongInput}` is not a valid action. Please try again with one of the following:\n" +
                                 $"`mute`, `kick`, `ban`, `shadowban`.");

            await SendEmbedAsync(embed);
        }

        private Embed GetStageOneEmbed()
        {
            return new KaguyaEmbedBuilder(KaguyaColors.NeonPurple)
                   .WithTitle("Anti Raid: User Threshold")
                   .WithDescription($"{Context.User.Mention} Okay, let's setup the anti raid for your server.\n\n" +
                                    "Please respond with how many users should join within a short time frame " +
                                    "for it to be considered a raid in your server.\n\n" +
                                    "Example: `5` or `20`.")
                   .Build();
        }

        private Embed GetStageTwoEmbed(uint userThreshold)
        {
            return new KaguyaEmbedBuilder(KaguyaColors.NeonPurple)
                   .WithTitle("Anti Raid: Duration Threshold")
                   .WithDescription($"{Context.User.Mention} Great. Now, what is the duration (in seconds) that {userThreshold:N0} users joining " +
                                    $"your server is considered a raid?\n\n" +
                                    "Example: `5` or `20`.")
                   .Build();
        }

        private Embed GetStageThreeEmbed(uint userThreshold, uint seconds)
        {
            return new KaguyaEmbedBuilder(KaguyaColors.NeonPurple)
                   .WithTitle("Anti Raid: Action")
                   .WithDescription($"{Context.User.Mention} Okay, so you've now told me that " + $"{userThreshold} users who join within {seconds} seconds ".AsBold() +
                                    $"of each other is to be flagged as a raid.\n\n" +
                                    $"What action should I perform on these users?\n\n" +
                                    $"Respond with: `mute`, `kick`, `ban`, or `shadowban`.\n" +
                                    $"Shadowbanned users cannot interact with the server in any way (cannot see channels, chats, other members, etc.)".AsItalics())
                   .Build();
        }

        private Embed GetStageFourConfirmationEmbed(AntiraidAction action)
        {
            return new KaguyaEmbedBuilder(KaguyaColors.Magenta)
                   .WithTitle("Anti Raid: Temporary Action Setup")
                   .WithDescription($"{Context.User.Mention} It looks like you've set an action that can also be lifted after a duration.\n" +
                                    $"Please select an option:\n" +
                                    $"✅ - Setup a punishment duration.\n" +
                                    $"{_commonEmotes.RedCrossEmote} - Apply indefinite punishment to all raiders (default).")
                   .Build();
        }

        private Embed GetStageFiveEmbed()
        {
            return new KaguyaEmbedBuilder(KaguyaColors.NeonPurple)
                   .WithTitle("Anti Raid: Punishment Duration")
                   .WithDescription($"{Context.User.Mention} How long should the user be punished?\n\n" +
                                    $"Enter a duration, (minimum 5 minutes, maximum 1 year), with the following format:\n\n" +
                                    $"- `xdxhxmxs`, where `x` is a number and `d`, `h`, `m`, and `s` mean " +
                                    $"`days`, `hours`, `minutes`, and `seconds` respectively.\n\n" +
                                    $"Examples:\n" +
                                    $"`5d12h` = 5 days, 12 hours\n" +
                                    $"`30m` = 30 minutes\n" +
                                    $"`12d19h200m35s` = 12 days, 22 hours, 30 minutes, 35 seconds -- Kaguya does the math!")
                   .Build();
        }

        private Embed GetStageFiveErrorEmbed()
        {
            return GetBasicErrorEmbedBuilder("The time you provided was not valid. Please try again.\n\n" +
                                             "Examples:\n" +
                                             $"`5d12h` = 5 days, 12 hours\n" +
                                             $"`30m` = 30 minutes\n").Build();
        }

        private Embed GetFinalEmbed(AntiRaidConfig config, string serverCmdPrefix)
        {
            return new KaguyaEmbedBuilder(KaguyaColors.Green)
                   .WithTitle("Anti Raid: Setup Complete")
                   .WithDescription($"{Context.User.Mention} Awesome! I've configured the antiraid for your server. Here's what I've got:\n\n" +
                                    $"{GetAntiraidConfigString(config)}")
                   .WithFooter($"Turn this off at any time through the \"{serverCmdPrefix}antiraid -toggle\" command.\n" +
                               $"Use the \"{serverCmdPrefix}antiraid -setmsg\" command to set a message to be sent to punished users.")
                   .Build();
        }

        /// <summary>
        /// Shortcut method to remove id from the static concurrent collection
        /// </summary>
        /// <param name="id"></param>
        private static void Remove(ulong id)
        {
            _currentlyActiveSetups.TryRemove(id, out var _);
        }
    }
    
    // todo: Antiraid toggle command.
    // todo: Antiraid -s command to skip setup process.
}