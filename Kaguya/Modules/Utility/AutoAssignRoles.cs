using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core;
using Kaguya.Core.Embed;
using Kaguya.Core.Server_Files;
using System;
using System.Linq;
using System.Threading.Tasks;
using EmbedColor = Kaguya.Core.Embed.EmbedColor;

namespace Kaguya.Modules.Utility
{
    public class AutoAssignRoles : InteractiveBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();
        private readonly DiscordShardedClient _client = Global.client;
        Logger logger = new Logger();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("autoassign")]
        [Alias("aa")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AutoAssignAdd([Remainder]IRole role)
        {
            var guild = Servers.GetServer(Context.Guild);

            if (!guild.AutoAssignedRoles.Contains(role.Id))
            {
                guild.AutoAssignedRoles.Add(role.Id);
                Servers.SaveServers();
                embed.WithTitle($"Auto Assign Role");
                embed.WithDescription($"New role added to automatically assigned roles: {role.Mention}");
                embed.WithFooter("This will be assigned to users as soon as they join the server.");
                await BE();
            }
            else
            {
                embed.WithTitle($"Auto Assign Role");
                embed.WithDescription($"This role is already in the list of auto assigned roles!");
                embed.SetColor(EmbedColor.RED);
                await BE();
                return;
            }
        }

        [Command("autoassignremove")]
        [Alias("aar")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AutoAssignRemove([Remainder]IRole role)
        {
            var guild = Servers.GetServer(Context.Guild);

            if (guild.AutoAssignedRoles.Contains(role.Id))
            {
                guild.AutoAssignedRoles.Remove(role.Id);
                Servers.SaveServers();
                embed.WithTitle($"Auto Assign Role");
                embed.WithDescription($"Role removed from list of auto assigned roles: {role.Mention}");
                await BE();
            }
            else
            {
                embed.WithTitle($"Auto Assign Role");
                embed.WithDescription($"This role is not in the list of auto assignable roles!");
                embed.SetColor(EmbedColor.RED);
                await BE();
                return;
            }
        }

        [Command("autoassignclear")]
        [Alias("aac")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AutoAssignClear()
        {
            var guild = Servers.GetServer(Context.Guild);

            guild.AutoAssignedRoles.Clear();
            Servers.SaveServers();
            embed.WithTitle($"Auto Assign Role");
            embed.WithDescription($"The list of auto assigned roles has been cleared.");
            embed.SetColor(EmbedColor.VIOLET);
            await BE();
        }

        [Command("autoassignview")]
        [Alias("aav")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AutoAssignView()
        {
            var guild = Servers.GetServer(Context.Guild);
            var roles = Context.Guild.Roles;
            string roleNames = "";

            foreach (var roleID in guild.AutoAssignedRoles)
            {
                roleNames += $"\n{roles.FirstOrDefault(x => x.Id == roleID).Mention}";
            }

            if (roleNames == "")
                roleNames = "No roles found.";

            embed.WithTitle($"Auto Assignable Roles");
            embed.WithDescription($"\n{roleNames}");
            embed.SetColor(EmbedColor.VIOLET);
            await BE();
        }

        public static async Task AutoAssignRole(SocketGuildUser user)
        {
            Logger logger = new Logger();
            var guild = Servers.GetServer(user.Guild);
            var roles = user.Guild.Roles;
            var autoassignedroles = guild.AutoAssignedRoles;

            if (autoassignedroles.Count > 0)
            {
                foreach (var roleID in autoassignedroles)
                {
                    try
                    {
                        await user.AddRoleAsync(roles.FirstOrDefault(x => x.Id == roleID));
                    }
                    catch (Exception e)
                    {
                        logger.ConsoleInformationAdvisory($"Failed to auto assign role {roles.FirstOrDefault(x => x.Id == roleID)} " +
                            $"to user {user} in guild {user.Guild}" +
                            $"\nException: {e.Message}");
                    }
                }
            }
        }
    }
}
