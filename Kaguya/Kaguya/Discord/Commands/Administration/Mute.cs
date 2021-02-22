using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Humanizer.Localisation;
using Interactivity;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Overrides.Extensions;
using Kaguya.Discord.Parsers;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Exceptions;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands.Administration
{
    [Module(CommandModule.Administration)]
    [Group("mute")]
    [Alias("m")]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    [RequireUserPermission(GuildPermission.MuteMembers)]
    [RequireUserPermission(GuildPermission.DeafenMembers)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.MuteMembers)]
    [RequireBotPermission(GuildPermission.DeafenMembers)]
    public class Mute : KaguyaBase<Mute>
    {
        private readonly ILogger<Mute> _logger;
        private readonly AdminActionRepository _adminActionRepository;
        private readonly KaguyaServerRepository _kaguyaServerRepository;
        private readonly InteractivityService _interactivityService;

        public Mute(ILogger<Mute> logger, AdminActionRepository adminActionRepository, KaguyaServerRepository kaguyaServerRepository, 
            InteractivityService interactivityService) : base(logger)
        {
            _logger = logger;
            _adminActionRepository = adminActionRepository;
            _kaguyaServerRepository = kaguyaServerRepository;
            _interactivityService = interactivityService;
        }

        [Command(RunMode = RunMode.Async)]
        [Summary("Mutes a user indefinitely with an optional reason. The mute persists until the " +
                 "user is unmuted via the `mute -u` command. If the mute role is manually removed from the user " +
                 "by a moderator, the mute will not be automatically reapplied.")]
        [Remarks("<user> [reason]")]
        [Example("@User#0000 Being really spammy in chat.")]
        public async Task MuteCommand(SocketGuildUser user, [Remainder]string reason = null)
        {
            KaguyaServer server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
            await MuteUserAsync(user, null, reason, server);
        }
        
        [Command("-t", RunMode = RunMode.Async)]
        [Summary("Mutes a user for a specified duration, with an optional reason.")]
        [Remarks("<user> <duration> [reason]")]
        [Example("@User#0000 30m Being really spammy in chat.")]
        [Example("@User#0000 1d16h35m25s")]
        // ReSharper disable once MethodOverloadWithOptionalParameter
        public async Task MuteCommand(SocketGuildUser user, string duration, [Remainder]string reason = null)
        {
            KaguyaServer server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);

            var timeParser = new TimeParser(duration);
            var parsedDuration = timeParser.ParseTime();

            if (parsedDuration == TimeSpan.Zero)
            {
                throw new TimeParseException(duration);
            }
            
            DateTimeOffset? muteExpiration = DateTimeOffset.Now.Add(parsedDuration);

            await MuteUserAsync(user, muteExpiration, reason, server);
        }

        [Command("-u")]
        [Summary("Unmutes a user.")]
        [Remarks("<user>")]
        public async Task UnmuteUserCommand(SocketGuildUser user)
        {
            KaguyaServer server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
            IRole muteRole = await GetMuteRoleAsync(server);
            bool isMuted = await UserIsCurrentlyMutedAsync(user, muteRole);

            if (!isMuted)
            {
                await SendBasicErrorEmbedAsync("This user is not muted.");

                return;
            }

            IList<AdminAction> allUserMutes = await GetUnexpiredMutesAsync(user.Id, server.ServerId);
            await _adminActionRepository.ForceExpireRangeAsync(allUserMutes);
            
            // Remove mute role from user, if applicable.
            if (user.Roles.Any(x => x.Id == muteRole.Id))
            {
                try
                {
                    await user.RemoveRoleAsync(muteRole);
                }
                catch (Exception e)
                {
                    await SendBasicErrorEmbedAsync($"An error occurred when trying to remove {user.Mention}'s mute role:\n" +
                                                   $"Error message: {e.Message.AsBold()}");

                    return;
                }
            }

            await SendBasicSuccessEmbedAsync($"Unmuted user {user.Mention}.");
        }

        [Command("-sync")]
        [Summary("Syncs all text, voice, and category channel permissions for this server's mute role. " +
                 "This should be used after adding new public channels so that muted users won't be able to type in them.")]
        public async Task MuteSyncCommand()
        {
            var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
            var muteRole = await GetMuteRoleAsync(server);
            var embedFields = await SetMutePermissionsAsync(muteRole);

            if (!embedFields.Any())
            {
                await SendBasicErrorEmbedAsync("All mute permissions are already synced!");

                return;
            }

            var embed = GetBasicSuccessEmbedBuilder($"Synced permissions for the mute role {muteRole.Mention}.", true)
                        .WithFields(embedFields)
                        .Build();

            await SendEmbedAsync(embed);
        }

        private async Task MuteUserAsync(SocketGuildUser user, DateTimeOffset? expiration, string reason, KaguyaServer server)
        {
            var adminAction = new AdminAction
            {
                ServerId = Context.Guild.Id,
                ModeratorId = Context.User.Id,
                ActionedUserId = user.Id,
                Action = AdminAction.MuteAction,
                Reason = reason,
                Expiration = expiration,
                Timestamp = DateTimeOffset.Now,
                HasTriggered = expiration.HasValue ? false : null // We specify this value if the user is temporarily actioned. Otherwise, leave it null.
            };

            bool muteRoleExists = DetermineIfMuteRoleExists(server);
            bool updateServer = false;
            
            IRole muteRole = await GetMuteRoleAsync(server);

            // We want to confirm with the user whether they want to overwrite the existing mute or leave the existing one.
            if (await UserIsCurrentlyMutedAsync(user, muteRole))
            {
                await SendConfirmationMessageAsync(user, expiration);
            }
            
            await _adminActionRepository.InsertAsync(adminAction); // Very earliest we can insert to DB.
            
            List<EmbedFieldBuilder> permissionFields = new();
            
            try
            {
                permissionFields = await SetMutePermissionsAsync(muteRole);
            }
            catch (Exception e)
            {
                await SendBasicErrorEmbedAsync("Warning: Failed to complete permission overwrite execution process. This error occurs from a " +
                                               $"lack of permissions.\n\nError: {e.Message.AsBold()}\n\n" +
                                               $"The mute operation will still continue. " + "Use the ".AsBold() + "mute -sync".AsCodeBlockSingleLine().AsBold() + " " + 
                                               "command after updating my permissions to continue.".AsBold());
            }
            
            try
            {
                await user.AddRoleAsync(muteRole);
            }
            catch (Exception e)
            {
                await SendBasicErrorEmbedAsync($"Failed to add role {muteRole.ToString().AsBold()} to user {user.ToString().AsBold()}.\nReason: {e.ToString().AsBold()}");

                return;
            }

            if (!muteRoleExists)
            {
                updateServer = true;
                server.MuteRoleId = muteRole.Id;
            }

            if (updateServer)
            {
                await _kaguyaServerRepository.UpdateAsync(server);
            }

            Embed embed = GetFinalEmbed(user, expiration, reason, permissionFields);
            await SendEmbedAsync(embed);
        }

        private async Task SendConfirmationMessageAsync(SocketGuildUser user, DateTimeOffset? expiration)
        {
            IList<AdminAction> currentUserMutes = await GetUnexpiredMutesAsync(user.Id, Context.Guild.Id);

            if (!currentUserMutes.Any())
                return;
            
            bool permanentMute = currentUserMutes.Any(x => !x.Expiration.HasValue);

            if (!permanentMute)
            {
                currentUserMutes = currentUserMutes.OrderByDescending(x => x.Expiration ?? DateTime.MinValue).ToList();
            }

            AdminAction longestMute = permanentMute ? currentUserMutes.First(x => !x.Expiration.HasValue) : currentUserMutes[0];

            if (longestMute == null)
            {
                return;
            }
            
            string oldMuteDurationStr = (permanentMute ? "never".AsBold() : longestMute.Expiration.Humanize()).Humanize(LetterCasing.Sentence).AsBold();
            string newMuteDurationStr = (!expiration.HasValue 
                ? "permanent" 
                : (expiration.Value - DateTimeOffset.Now).Humanize(3, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day) + " from now").AsBold();

            string reasonStr = (longestMute.Reason ?? "<No reason provided>").AsItalics();
            
            SocketGuildUser oldMod = Context.Guild.GetUser(longestMute.ModeratorId);

            var overwriteEmbed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
            {
                Description = $"This user is already muted. Would you like to overwrite their current mute? Details of " +
                              $"the current mute are described below:\n" +
                              $"- Expiration: [current: {oldMuteDurationStr} | new: {newMuteDurationStr}]\n" +
                              $"- Reason: {reasonStr}\n" +
                              $"- Moderator: " +
                              (oldMod?.Mention ?? "Not found".AsItalics()) +
                              "\n\n" +
                              "Response will expire in 60 seconds, defaulting to ✅.".AsItalics() +
                              "\n" +
                              "Note: Overwriting does not erase mute history.".AsItalics() +
                              "\n\n" +
                              $"✅ = Replace old duration with new. (default)\n" +
                              $"❌ = Don't replace old. User will be unmuted at latest possible time."
            };
                
            var result = await _interactivityService.SendConfirmationAsync(overwriteEmbed, Context.Channel, TimeSpan.FromSeconds(60));

            // If the user wants to overwrite...
            if (result.Value)
            {
                // We force expire as we want to keep the mute reason history.
                // Forcing expiration ensures it won't be actioned on by any background services.
                await _adminActionRepository.ForceExpireRangeAsync(currentUserMutes);
                await SendBasicSuccessEmbedAsync("Okay, I'll replace the old mute duration with the one you just provided.");
            }
            else
            {
                await SendBasicSuccessEmbedAsync("Okay, I'll insert this and log it, but if the user currently has mutes that expire later " +
                                                 "than what you provided, they will be unmuted at that time.\n" +
                                                 "Use the `mute -status` command to view this information.");
            }
        }

        private bool DetermineIfMuteRoleExists(KaguyaServer server)
        {
            if (!server.MuteRoleId.HasValue)
            {
                return false;
            }
            
            SocketRole muteRole = Context.Guild.GetRole(server.MuteRoleId.Value);
            return muteRole != null;
        }

        private async Task<IRole> GetMuteRoleAsync(KaguyaServer server)
        {
            IRole match = Context.Guild.GetRole(server.MuteRoleId ?? 0) ?? await CreateMuteRoleAsync();
            return match;
        }
        
        private async Task<IRole> CreateMuteRoleAsync()
        {
            _logger.LogDebug($"Mute role created in guild {Context.Guild.Id}. Guild roles: {Context.Guild.Roles.Humanize()}");
            return await Context.Guild.CreateRoleAsync("kaguya-mute", GuildPermissions.None, KaguyaColors.Default, false, false);
        }

        private async Task<List<EmbedFieldBuilder>> SetMutePermissionsAsync(IRole role)
        {
            List<EmbedFieldBuilder> fieldBuilders = new();
            List<SocketTextChannel> textChannelsToUpdate = new();
            List<SocketCategoryChannel> categoriesToUpdate = new();
            List<SocketVoiceChannel> voiceChannelsToUpdate = new();
            
            foreach (SocketTextChannel textChannel in Context.Guild.TextChannels)
            {
                if (!textChannel.GetPermissionOverwrite(role).HasValue)
                {
                    textChannelsToUpdate.Add(textChannel);
                }
            }
            
            foreach (SocketCategoryChannel category in Context.Guild.CategoryChannels)
            {
                if (!category.GetPermissionOverwrite(role).HasValue)
                {
                    categoriesToUpdate.Add(category);
                }
            }
            
            foreach (SocketVoiceChannel vc in Context.Guild.VoiceChannels)
            {
                if (!vc.GetPermissionOverwrite(role).HasValue)
                {
                    voiceChannelsToUpdate.Add(vc);
                }
            }

            if (textChannelsToUpdate.Any())
            {
                string s = textChannelsToUpdate.Count == 1 ? default : "s";
                fieldBuilders.Add(new EmbedFieldBuilder
                {
                    Name = "Channel Permissions",
                    Value = $"Denied all permissions for role {role.ToString().AsBold()} in {textChannelsToUpdate.Count.ToString("N0").AsBold()} text channel{s}."
                });
            }
            
            if (categoriesToUpdate.Any())
            {
                string s = textChannelsToUpdate.Count == 1 ? default : "s";
                fieldBuilders.Add(new EmbedFieldBuilder
                {
                    Name = "Category Permissions",
                    Value = $"Denied all permissions for role {role.ToString().AsBold()} in {categoriesToUpdate.Count.ToString("N0").AsBold()} categorie{s}."
                });
            }
            
            if (voiceChannelsToUpdate.Any())
            {
                string s = textChannelsToUpdate.Count == 1 ? default : "s";
                fieldBuilders.Add(new EmbedFieldBuilder
                {
                    Name = "Voice Channel Permissions",
                    Value = $"Denied all permissions for role {role.ToString().AsBold()} in {voiceChannelsToUpdate.Count.ToString("N0").AsBold()} voice channel{s}."
                });
            }

            List<IGuildChannel> finalCollection = new();
            
            finalCollection.AddRange(textChannelsToUpdate);
            finalCollection.AddRange(voiceChannelsToUpdate);
            finalCollection.AddRange(categoriesToUpdate);

            if (finalCollection.Any())
            {
                await ReplyAsync($"{Context.User.Mention} Processing channel permissions. Please wait...");
                await Task.Run(async () =>
                {
                    foreach (IGuildChannel channel in finalCollection)
                    {
                        await channel.AddPermissionOverwriteAsync(role, GetMuteOverwritePermissions());
                    }
                });
            }

            return fieldBuilders;
        }

        // todo: Permissions are hardcoded for the mute role. Eventually add support for modifying this collection.
        /// <summary>
        /// A <see cref="OverwritePermissions"/> for any mute role created by Kaguya.
        /// </summary>
        /// <returns></returns>
        public static OverwritePermissions GetMuteOverwritePermissions()
        {
            return new OverwritePermissions(PermValue.Deny, addReactions: PermValue.Deny, sendMessages: PermValue.Deny, muteMembers: PermValue.Deny,
                useVoiceActivation: PermValue.Deny, attachFiles: PermValue.Deny, embedLinks: PermValue.Deny, connect: PermValue.Deny, speak: PermValue.Deny,
                useExternalEmojis: PermValue.Deny, viewChannel: PermValue.Inherit);
        }

        private async Task<bool> UserIsCurrentlyMutedAsync(SocketGuildUser user, IRole muteRole)
        {
            if (user.Roles.Any(x => x.Id == muteRole.Id))
            {
                return true;
            }
            // unexpired = null expiration or expiration that has not already expired.
            IList<AdminAction> unexpiredUserMutes = await GetUnexpiredMutesAsync(user.Id, Context.Guild.Id);

            return unexpiredUserMutes.Any();
        }

        private async Task<IList<AdminAction>> GetUnexpiredMutesAsync(ulong userId, ulong serverId)
        {
            return await _adminActionRepository.GetAllUnexpiredAsync(userId, serverId, AdminAction.MuteAction);
        }
        
        private Embed GetFinalEmbed(SocketGuildUser target, DateTimeOffset? expiration, string reason, List<EmbedFieldBuilder> fields)
        {
            string durationStr = expiration.HasValue 
                ? $" for {(expiration.Value - DateTimeOffset.Now).Humanize(3, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day).AsBold()}" 
                : string.Empty;

            string reasonStr = reason == null ? "<No reason provided>".AsBold() : reason.AsBold();

            return new KaguyaEmbedBuilder(KaguyaColors.Purple)
                   .WithDescription($"{Context.User.Mention} Muted user {target.Mention}{durationStr}." +
                                    $"\nReason: {reasonStr}")
                   .WithFooter("To unmute this user, use the mute -u command.")
                   .WithFields(fields)
                   .Build();
        }
    }
}