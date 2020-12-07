using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord.Commands;
using Kaguya.Discord.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Interactivity;
using Interactivity.Confirmation;
using Kaguya.Discord.DiscordExtensions;

namespace Kaguya.Discord.Commands.Administration
{
    [Module(CommandModule.Administration)]
    [Group("role")]
    [Alias("r")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public class Role : KaguyaBase<Role>
    {
        private readonly ILogger<Role> _logger;
        private readonly InteractivityService _interactivityService;

        public Role(ILogger<Role> logger, InteractivityService interactivityService) : base(logger)
        {
            _logger = logger;
            _interactivityService = interactivityService;
        }

        [Command("-add")]
        [Alias("-a")]
        [Summary("Adds a role to a user. If the role name has spaces, wrap it in \"quotation marks.\"")]
        [Remarks("<role> <user>")]
        public async Task AddRoleCommand(SocketRole role, SocketGuildUser guildUser)
        {
            try
            {
                await guildUser.AddRoleAsync(role);
                await SendBasicSuccessEmbedAsync($"Successfully added role {role.Name.AsBold()} to {guildUser.ToString().AsBold()}");
            }
            catch (Exception e)
            {
                await SendBasicErrorEmbedAsync($"Failed to add role {role.Name.AsBold()} to user {guildUser.ToString().AsBold()}.\n" +
                                               $"Error Message: {e.Message.AsBold()}");
            }
        }

        [Command("-remove")]
        [Alias("-rem", "-r")]
        [Summary("Removes a role from a user. If the role name has spaces, wrap it in \"quotation marks.\"")]
        [Remarks("<role> <user>")]
        public async Task RemoveRoleCommand(SocketRole role, SocketGuildUser guildUser)
        {
            try
            {
                await guildUser.RemoveRoleAsync(role);
                await SendBasicSuccessEmbedAsync($"Successfully removed role {role.Name.AsBold()} from {guildUser.ToString().AsBold()}");
            }
            catch (Exception e)
            {
                await SendBasicErrorEmbedAsync($"Failed to remove role {role.Name.AsBold()} from user {guildUser.ToString().AsBold()}.\n" +
                                               $"Error Message: {e.Message.AsBold()}");
            }
        }
        
        [Command("-addlist")]
        [Alias("-al")]
        [Summary("Adds a list of roles to the user. If any roles have spaces, wrap each of them in \"quotation marks\".")]
        [Remarks("<user> <role 1> [...]")]
        public async Task AddRoleCommand(SocketGuildUser guildUser, params SocketRole[] roles)
        {
            string successStart = $"Roles added to {guildUser.ToString().AsBold()}:\n\n";
            string errorStart = "Roles that failed to add:\n\n";
            
            var successBuilder = new StringBuilder(successStart);
            var errorBuilder = new StringBuilder(errorStart);
            
            foreach (SocketRole role in roles)
            {
                try
                {
                    await guildUser.AddRoleAsync(role);
                    successBuilder.AppendLine("- " + role.Name.AsBold());
                }
                catch (Exception e)
                {
                    errorBuilder.AppendLine($"- {role.Name.AsBold()} : Error Message: {e.Message.AsBold()}");
                }
            }

            if (successBuilder.ToString() != successStart)
            {
                await SendBasicSuccessEmbedAsync(successBuilder.ToString());
            }

            if (errorBuilder.ToString() != errorStart)
            {
                await SendBasicErrorEmbedAsync(errorBuilder.ToString());
            }
        }
        
        [Command("-removelist")]
        [Alias("-remlist", "-reml", "-rl")]
        [Summary("Removes a list of roles from the user. If any roles have spaces, wrap each of them in \"quotation marks\".")]
        [Remarks("<user> <role 1> [...]")]
        public async Task RemoveRoleCommand(SocketGuildUser guildUser, params SocketRole[] roles)
        {
            string successStart = $"Roles removed from {guildUser.ToString().AsBold()}:\n\n";
            string noContainsStart = $"Roles user {guildUser.ToString().AsBold()} did not already have:\n\n";
            string errorStart = "Roles that failed to be removed:\n\n";
            
            var successBuilder = new StringBuilder(successStart);
            var didNotContainsBuilder = new StringBuilder(noContainsStart);
            var errorBuilder = new StringBuilder(errorStart);
            
            foreach (SocketRole role in roles)
            {
                try
                {
                    if (guildUser.Roles.Contains(role))
                    {
                        await guildUser.RemoveRoleAsync(role);
                        successBuilder.AppendLine("- " + role.Name.AsBold());
                    }
                    else
                    {
                        didNotContainsBuilder.AppendLine("- " + role.Name.AsBold());
                    }
                }
                catch (Exception e)
                {
                    errorBuilder.AppendLine($"{role.Name.AsBold()} : Error Message: {e.Message.AsBold()}");
                }
            }
            
            if (successBuilder.ToString() != successStart)
            {
                await SendBasicSuccessEmbedAsync(successBuilder.ToString());
            }

            if (errorBuilder.ToString() != errorStart)
            {
                await SendBasicErrorEmbedAsync(errorBuilder.ToString());
            }

            if (didNotContainsBuilder.ToString() != noContainsStart)
            {
                await SendBasicEmbed(didNotContainsBuilder.ToString(), Color.DarkMagenta);
            }
        }
        
        [Command("-clear")]
        [Summary("Removes all roles from the user.")]
        [Remarks("<user>")]
        public async Task ClearRolesFromUserCommand(SocketGuildUser user)
        {
            try
            {
                await user.RemoveRolesAsync(user.Roles.Where(x => !x.IsManaged && !x.IsEveryone));
            }
            catch (Exception e)
            {
                await SendBasicErrorEmbedAsync($"Failed to clear roles from user {user.ToString().AsBold()}.\n" +
                                               $"Error: {e.Message.AsBold()}");

                return;
            }

            await SendBasicSuccessEmbedAsync($"Cleared all roles from {user.ToString().AsBold()}");
        }

        [Command("-create")]
        [Alias("-c")]
        [Summary("Creates a role with the desired name and an optional color. If the " +
                 "name has spaces, be sure to wrap it in \"quotation marks\".\n" +
                 "If specifying a color, ensure that is one word only. Example colors:\n" +
                 "Lightblue, red, orange, darkpurple, darkmagenta, gold, yellow, lightred, pink, etc.")]
        [Remarks("<role name> [color]")]
        public async Task CreateRoleCommand(string roleName)
        {
            try
            {
                await Context.Guild.CreateRoleAsync(roleName, null, null, false, null);
            }
            catch (Exception e)
            {
                await SendBasicErrorEmbedAsync("Failed to create role " + roleName.AsBold() + $"\nError: {e.Message.AsBold()}");
            }
            
            await SendBasicSuccessEmbedAsync("Created role " + roleName.AsBold());
        }
        
        [Command("-create")]
        [Alias("-c")]
        public async Task CreateRoleCommand(string roleName, string colorname)
        {
            var color = System.Drawing.Color.FromName(colorname);
            var discordColor = new Color(color.R, color.G, color.B);
            
            try
            {
                await Context.Guild.CreateRoleAsync(roleName, null, discordColor, false, null);
            }
            catch (Exception e)
            {
                await SendBasicErrorEmbedAsync("Failed to create role " + roleName.AsBold() + $"\nError: {e.Message.AsBold()}");
            }
            
            await SendBasicSuccessEmbedAsync("Created role " + roleName.AsBold() + " with color " + color.Name.AsBold());
        }

        [Command("-delete")]
        [Alias("-d")]
        [Summary("Deletes the role from the server. If a role name has spaces, " +
                 "wrap it in \"quotation marks\".")]
        [Remarks("<role>")]
        public async Task DeleteRoleCommand(SocketRole role)
        {
            try
            {
                await role.DeleteAsync();
            }
            catch (Exception e)
            {
                await SendBasicErrorEmbedAsync($"Failed to delete role {role.Name.AsBold()} from the server.\n" +
                                               $"Error: {e.Message.AsBold()}");

                return;
            }

            await SendBasicSuccessEmbedAsync($"Deleted role {role.Name.AsBold()} from the server.");
        }
        
        [Command("-deletelist")]
        [Alias("-deletel", "-dl")]
        [Summary("Deletes a list of roles from the server. If a role name has spaces, " +
                 "wrap it in \"quotation marks\".")]
        [Remarks("<role> [role 2] [...]")]
        public async Task DeleteRoleCommand(params SocketRole[] roles)
        {
            var successBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();
            var finalBuilder = new StringBuilder();
            
            bool success = false;
            bool failure = false;
            
            foreach (var role in roles)
            {
                try
                {
                    await role.DeleteAsync();
                    successBuilder.AppendLine($"- {role.Name.AsBold()}");

                    success = true;
                }
                catch (Exception)
                {
                    errorBuilder.AppendLine($"- {role.Name.AsBold()}");
                    failure = true;
                }
            }

            if (success)
            {
                finalBuilder.AppendLine("Roles deleted:")
                            .AppendLine(successBuilder.ToString());
            }

            if (failure)
            {
                finalBuilder.AppendLine("Failed to delete roles:")
                            .AppendLine(errorBuilder.ToString());
            }

            Color color;
            if (success && failure)
            {
                color = Color.DarkMagenta;
            }
            else if (success && !failure)
            {
                color = Color.Green;
            }
            else if (!success && failure)
            {
                color = Color.Red;
            }
            else
            {
                color = default;
            }

            var embed = new KaguyaEmbedBuilder(color)
            {
                Description = finalBuilder.ToString()
            };

            await SendEmbedAsync(embed);
        }
        
        [Restriction(ModuleRestriction.PremiumOnly)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("-deleteall", RunMode = RunMode.Async)]
        [Summary("Deletes all roles in this server.")]
        public async Task DeleteAllRolesInGuildCommand()
        {
            Confirmation request = new ConfirmationBuilder()
                                   .WithContent(new PageBuilder().WithDescription("Are you sure that you wish to " + 
                                                                           "delete all roles in this server?".AsBoldItalics() + "\n" + 
                                                                           "This cannot be undone!")
                                                                 .WithColor(System.Drawing.Color.Yellow))
                                   .Build();

            var result = await _interactivityService.SendConfirmationAsync(request, Context.Channel);

            if (result.Value)
            {
                var rolesCollection = Context.Guild.Roles.Where(x => !x.IsEveryone && !x.IsManaged);

                SocketRole[] roles = rolesCollection as SocketRole[] ?? rolesCollection.ToArray();
                if (!roles.Any())
                {
                    await SendBasicErrorEmbedAsync("There are no roles that can be deleted.");

                    return;
                }
                
                await SendBasicSuccessEmbedAsync($"{Context.User.Mention} Processing {roles.Length:N0} roles...");
                
                int deleted = 0;
                int failed = 0;
                foreach (var role in roles)
                {
                    try
                    {
                        await role.DeleteAsync();
                        deleted++;
                    }
                    catch (Exception)
                    {
                        failed++;
                    }
                }

                await SendBasicSuccessEmbedAsync($"Successfully deleted {deleted:N0} roles.");

                if (failed != 0)
                {
                    await SendBasicErrorEmbedAsync($"Failed to delete {failed:N0} roles.");
                }
            }
            else
            {
                await SendBasicEmbed("No action will be taken.", Color.DarkBlue);
            }
        }
    }
}