using System;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Helpers;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class Shadowban : KaguyaBase
    {
        private const string SB_ROLE = "kaguya-shadowban";

        [AdminCommand]
        [Command("Shadowban")]
        [Alias("sb")]
        [Summary("Shadowbans a user, denying them of every possible channel permission, meaning " +
                 "they will no longer be able to view or interact with any voice channels.\n" +
                 "__**This command also strips the user of any roles they may have.**__")]
        [Remarks("<user>")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireUserPermission(GuildPermission.DeafenMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ShadowbanUser(SocketGuildUser user)
        {
            SocketGuild guild = Context.Guild;
            IRole role = guild.Roles.FirstOrDefault(x => x.Name == SB_ROLE);
            if (role == null)
            {
                await ReplyAsync($"{Context.User.Mention} Could not find role `{SB_ROLE}`. Creating...");
                RestRole newRole = await guild.CreateRoleAsync(SB_ROLE, GuildPermissions.None, null, false, false, null);
                role = newRole;
                await ReplyAsync($"{Context.User.Mention} Created role `{SB_ROLE}` with permissions: `none`.");
                await ReplyAsync($"{Context.User.Mention} Scanning permission overwrites for channels...");
            }
            
            try
            {
                await ScanChannelsForPermissions(role);

                if (user.Roles.Contains(role))
                {
                    await SendBasicErrorEmbedAsync($"{user.Mention} already has the role {role.Mention}.");
                    return;
                }
                
                await user.AddRoleAsync(role);
            }
            catch (Exception e)
            {
                throw new KaguyaSupportException("Failed to add `kaguya-mute` role to user!\n\n" +
                                                 $"Error Log: ```{e}```");
            }
            
            IEnumerable<SocketRole> roles = user.Roles.Where(x => !x.IsManaged && x.Name != "@everyone");
            await user.RemoveRolesAsync(roles);

            var successEmbed = new KaguyaEmbedBuilder
            {
                Description = $"`{user}` has been transported to the shadowlands...",
                Footer = new EmbedFooterBuilder
                {
                    Text = "In the shadowlands, users may not interact with any text or voice channel, " +
                           "or view who is in the server.\n\n" +
                           "Use the unshadowban command to undo this action."
                }
            };

            await ReplyAsync(embed: successEmbed.Build());
        }

        private async Task ScanChannelsForPermissions(IRole role)
        {
            int permissionCount = 0;
            foreach (SocketTextChannel ch in Context.Guild.TextChannels)
            {
                if (!ch.GetPermissionOverwrite(role).HasValue)
                {
                    permissionCount++;
                    await ch.AddPermissionOverwriteAsync(role, OverwritePermissions.DenyAll(ch));
                }
            }

            if (permissionCount > 0)
            {
                await ReplyAsync($"{Context.User.Mention} Modified {permissionCount:N0} {StringHelpers.SFormat("channel", permissionCount)}, " +
                                 $"denying all permissions for role `{SB_ROLE}`.");
            }
        }

        /// <summary>
        /// "Silently" shadowbans a user. This version does not send any chat messages,
        /// therefore it can be executed by the WarnHandler class. This should only be
        /// executed by the WarnHandler.
        /// </summary>
        /// <param name="user">The user to shadowban.</param>
        /// <returns></returns>
        public async Task AutoShadowbanUserAsync(SocketGuildUser user)
        {
            SocketGuild guild = user.Guild;
            SocketRole role = guild.Roles.FirstOrDefault(x => x.Name == SB_ROLE);
            
            if (role == null)
            {
                try
                {
                    await guild.CreateRoleAsync(SB_ROLE, GuildPermissions.None, null, false, false, null);
                    role = guild.Roles.FirstOrDefault(x => x.Name == SB_ROLE);
                }
                catch (Exception e)
                {
                    await ConsoleLogger.LogAsync(e, $"Failed to create shadowban role in guild {guild.Id}.");
                }
                
            }
            
            try
            {
                IEnumerable<SocketRole> roles = user.Roles.Where(x => !x.IsManaged && x.Name != "@everyone");
                await user.RemoveRolesAsync(roles);
                
                await user.AddRoleAsync(role);
            }
            catch (Exception e)
            {
                await ConsoleLogger.LogAsync(e, $"Failed to automatically shadowban user {user.Id} in guild {guild.Id}.");
            }
        }
    }
}