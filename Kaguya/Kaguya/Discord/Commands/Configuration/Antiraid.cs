using System;
using System.Collections.Concurrent;
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
    [Restriction(ModuleRestriction.PremiumServer)]
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
    public class Antiraid : KaguyaBase<Antiraid>
    {
        private readonly ILogger<Antiraid> _logger;
        private readonly InteractivityService _interactivityService;
        private readonly KaguyaServerRepository _kaguyaServerRepository;
        private readonly AntiraidConfigRepository _antiraidConfigRepository;
        private readonly CommonEmotes _commonEmotes;
        private static readonly ConcurrentDictionary<ulong, bool> _currentlyActiveSetups = new();

        public Antiraid(ILogger<Antiraid> logger, InteractivityService interactivityService,
            KaguyaServerRepository kaguyaServerRepository, AntiraidConfigRepository antiraidConfigRepository, 
            CommonEmotes commonEmotes) : base(logger)
        {
            _logger = logger;
            _interactivityService = interactivityService;
            _kaguyaServerRepository = kaguyaServerRepository;
            _antiraidConfigRepository = antiraidConfigRepository;
            _commonEmotes = commonEmotes;
        }

        [Priority(1)]
        [Command(RunMode = RunMode.Async)]
        [Summary("Displays an interactive setup utility for configuring the Kaguya Antiraid service.\n\n" +
                 "You can also use this command with parameters to bypass the interactive setup entirely.\n\n" +
                 "Parameters:\n" +
                 "`<# users>` - The threshold of users who join before it is classified as a raid.\n" +
                 "`<# seconds>` - The rate of frequency, in seconds, that `<# users>` users join the server " +
                 "before it is classified as a raid.\n" +
                 "`<action>` - How to punish the raiders. `kick`, `mute`, `ban`, and `shadowban` are the only valid responses.\n" +
                 "`[punishment duration]` - Optional time duration to punish users for, if an action is able to be made temporary. `kick`s are " +
                 "unable to be made temporary, because a user has to rejoin a server manually. Example time: `30m` or `5d2h25m30s`.")]
        [Remarks("\n<# users> <# seconds> <action> [punishment duration]")]
        [Example("")]
        [Example("5 20 kick")]
        [Example("5 20 ban")]
        [Example("5 20 mute 7d")]
        [Example("5 20 shadowban 2h30m")]
        public async Task AntiraidSetupCommand()
        {
            if (_currentlyActiveSetups.ContainsKey(Context.Guild.Id))
            {
                await SendSetupConflictErrorEmbedAsync();

                return;
            }

            Add(Context.Guild.Id);
            
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
                ServerId = Context.Guild.Id,
                ConfigEnabled = true
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
                
                if (!ValidateActionInput(userMessage.Value.Content))
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
                    
                    while (true)
                    {
                        var nextMessage = await _interactivityService.NextMessageAsync(x => x.Author == Context.User, null, TimeSpan.FromMinutes(2));

                        if (!ValidatePunishmentDurationInput(nextMessage.Value.Content))
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
                            newArConfig.PunishmentLength = GetPunishmentDurationTimeSpan(nextMessage.Value.Content);

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

        [Priority(0)]
        [Command(RunMode = RunMode.Async)]
        public async Task AntiraidSetupCommand(uint userThreshold, uint secondsThreshold, string action, string timeString = null)
        {
            if (_currentlyActiveSetups.ContainsKey(Context.Guild.Id))
            {
                await SendSetupConflictErrorEmbedAsync();

                return;
            }
         
            Add(Context.Guild.Id);
            
            var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);

            if (!await ValidateUserThreshold(userThreshold))
            {
                Remove(Context.Guild.Id);
                
                return;
            }

            if (!await ValidateSecondsThreshold(secondsThreshold))
            {
                Remove(Context.Guild.Id);
                
                return;
            }

            if (!ValidateActionInput(action))
            {
                Remove(Context.Guild.Id);
                
                return;
            }

            AntiraidAction parsedAction = GetAntiraidActionEnum(action);
            if (parsedAction == AntiraidAction.Kick && timeString != null)
            {
                Remove(Context.Guild.Id);
                
                await SendBasicErrorEmbedAsync("You cannot specify a punishment duration with the kick action.");
                
                return;
            }

            TimeSpan? parsedTime = null;
            if (parsedAction != AntiraidAction.Kick && timeString != null)
            {
                if (!ValidatePunishmentDurationInput(timeString))
                {
                    Remove(Context.Guild.Id);

                    await SendBasicErrorEmbedAsync($"I couldn't parse your punishment duration. Please review the `{server.CommandPrefix}help antiraid` command " +
                                                   $"for proper usage examples.\n\n" +
                                                   $"The duration must be at least 5 seconds and no more than 1 year.");
                    return;
                }
                else
                {
                    parsedTime = GetPunishmentDurationTimeSpan(timeString);
                }
            }

            bool overwrite = false;
            var curArConfig = await _antiraidConfigRepository.GetAsync(Context.Guild.Id);
            if (curArConfig != null)
            {
                if (!await ConfirmConfigOverwriteAsync(curArConfig))
                {
                    Remove(Context.Guild.Id);

                    return;
                }

                overwrite = true;
            }

            var newArConfig = new AntiRaidConfig
            {
                ServerId = Context.Guild.Id,
                UserThreshold = userThreshold,
                Seconds = secondsThreshold,
                Action = parsedAction,
                PunishmentLength = parsedTime,
                AntiraidPunishmentDirectMessage = overwrite ? curArConfig.AntiraidPunishmentDirectMessage : null,
                ConfigEnabled = true,
                PunishmentDmEnabled = false
            };

            if (newArConfig.AntiraidPunishmentDirectMessage != null)
            {
                newArConfig.PunishmentDmEnabled = true;
            }

            Remove(Context.Guild.Id);
            
            await _antiraidConfigRepository.InsertOrUpdateAsync(newArConfig);
            await SendEmbedAsync(GetFinalEmbed(newArConfig, server.CommandPrefix));
        }

        [Command("-toggle")]
        [Alias("-t")]
        [Summary("Toggles the antiraid service on or off for this server.")]
        public async Task AntiraidToggleCommand()
        {
            var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
            var curConfig = await _antiraidConfigRepository.GetAsync(Context.Guild.Id);

            if (curConfig == null)
            {
                await SendBasicErrorEmbedAsync("There is no antiraid configuration for this server. Set one up with " +
                                               "the " +
                                               $"{server.CommandPrefix}antiraid".AsCodeBlockSingleLine() +
                                               " command.");

                return;
            }

            bool enabled = curConfig.ConfigEnabled;
            
            string newState = !enabled ? "enabled" : "disabled";
            curConfig.ConfigEnabled = !enabled;

            await _antiraidConfigRepository.UpdateAsync(curConfig);
            
            await SendBasicEmbedAsync($"This antiraid config is now " + newState.AsBold() + ".\n\n" +
                                      $"Config:\n{GetAntiraidConfigString(curConfig)}", KaguyaColors.LightYellow);
        }

        [Command("-setmsg")]
        [Alias("-msg")]
        [Summary("Sets and enables the antiraid direct messages (DMs) for this server. Antiraid DMs " +
                 "are messages that get sent to each individually actioned user by the antiraid service.\n\n" +
                 "This is nice because in the event of a false-positive (i.e. legitimate user joins the server while a raid is in progress), " +
                 "the legitimate user can be sent a custom DM with an invite link or ban appeal form, for example.\n\n" +
                 "__Keywords:__ (use these keywords to fill-in data dynamically)\n" +
                 "- `{USERNAME}` - Name of user e.g. Kaguya\n" +
                 "- `{USERMENTION}` - Mentions user e.g. @Kaguya#0000 (highlighted, etc.)\n" +
                 "- `{ACTION}` - Name of action in past-tense form e.g. `banned`, `kicked`, `muted`, or `shadowbanned`\n" +
                 "- `{SERVERNAME}` - Name of the server they were actioned from.\n\n" +
                 "__Notice:__\n" +
                 "Servers with Antiraid DMs that violate Discord's " + Global.DiscordTermsLink + " or " + Global.DiscordCommunityGuidelinesLink + " " +
                 "will receive a permanent, irrevocable blacklist from using Kaguya.")]
        [Remarks("<message>")]
        [Example("Dear {USERNAME},\n\nYou were flagged as part of a raid and were {ACTION} from {SERVERNAME}.\n\n" +
                 "Ban appeal form: (some URL)\n" +
                 "Invite link: (some URL)\n\n" +
                 "We apologize for the inconvenience, we hope to see you in our server soon!", ExampleStringFormat.CodeblockMultiLine)]
        public async Task SetAntiraidMessageCommand([Remainder]string message)
        {
            var profanityFilter = new ProfanityFilter.ProfanityFilter();
            if (profanityFilter.ContainsProfanity(message))
            {
                await SendBasicErrorEmbedAsync("You filthy animal...try again with a cleaner message.");
                
                return;
            }

            var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
            var curConfig = await _antiraidConfigRepository.GetAsync(Context.Guild.Id);
            if (curConfig == null)
            {
                await SendBasicErrorEmbedAsync("I'm sorry, but you need to setup the antiraid configuration " +
                                               $"for this server first through the `{server.CommandPrefix}antiraid` " +
                                               $"command before configuring a DM message.");

                return;
            }

            curConfig.AntiraidPunishmentDirectMessage = message;

            await SendBasicSuccessEmbedAsync($"I've set this server's Antiraid DM to the following:\n\n" +
                                             $"{message}\n\n" +
                                             $"Any keywords used will be replaced with the correct information at the time " +
                                             $"of a raid.");
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
            
            if (config.Action == AntiraidAction.Ban || config.Action == AntiraidAction.Mute || config.Action == AntiraidAction.Shadowban)
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

            if (!await ValidateUserThreshold(userThreshold))
                return false;

            return true;
        }

        private async Task<bool> ValidateUserThreshold(uint userThreshold)
        {
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

        private async Task<bool> ValidateSecondsThreshold(uint secondsThreshold)
        {
            if (secondsThreshold < 5 || secondsThreshold > 600)
            {
                await SendBasicErrorEmbedAsync("The seconds threshold must be at least 5 and no greater than 600.");
                return false;
            }

            return true;
        }

        private bool ValidateActionInput(string input)
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

        private bool ValidatePunishmentDurationInput(string input)
        {
            var timeParser = new TimeParser(input);
            var parsedTime = timeParser.ParseTime();

            return parsedTime != TimeSpan.Zero && parsedTime >= TimeSpan.FromSeconds(5) && parsedTime <= TimeSpan.FromDays(365);
        }

        private TimeSpan GetPunishmentDurationTimeSpan(string input) => new TimeParser(input).ParseTime();

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
        
        private async Task SendSetupConflictErrorEmbedAsync() => await SendBasicErrorEmbedAsync("There is already an active antiraid setup running in this server. Please complete the first " +
                                                                                           "setup before beginning this one.");

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
        
        private static void Add(ulong id) => _currentlyActiveSetups.GetOrAdd(id, true);
    }
}