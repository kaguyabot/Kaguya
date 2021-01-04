using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Interactivity;
using Interactivity.Confirmation;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Attributes;
using Kaguya.Discord.Attributes.Enums;
using Kaguya.Discord.DiscordExtensions;
using Microsoft.Extensions.Logging;

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
        private readonly KaguyaServerRepository _kaguyaServerRepository;

        public Role(ILogger<Role> logger, InteractivityService interactivityService, 
            KaguyaServerRepository kaguyaServerRepository) : base(logger)
        {
            _logger = logger;
            _interactivityService = interactivityService;
            _kaguyaServerRepository = kaguyaServerRepository;
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
                await SendBasicEmbedAsync(didNotContainsBuilder.ToString(), Color.DarkMagenta);
            }
        }

        [Command("-assign")]
        [Alias("-a")]
        [Summary("Mass-assigns a role to a single user or a list of users. If any users are not found, this command " +
                 "will not apply the role to any users and will need to be re-run. If a user has a space in their name, use \"quotation marks\" " +
                 "or mention them.")]
        [Remarks("<role> <user> {...}")]
        public async Task AssignRoleCommand(SocketRole role, params SocketGuildUser[] users)
        {
            string successBase = $"Successfully assigned role {role.ToString().AsBold()} to:\n\n";
            const string ERROR_BASE = "Errors:\n\n";
            
            var successBuilder = new StringBuilder(successBase);
            var errorBuilder = new StringBuilder(ERROR_BASE);
            var finalBuilder = new StringBuilder();

            List<string> successUsers = new List<string>();

            foreach (SocketGuildUser user in users)
            {
                try
                {
                    await user.AddRoleAsync(role);
                    successUsers.Add(user.Mention);
                }
                catch (Exception e)
                {
                    errorBuilder.AppendLine($"Role assignment for {user.ToString().AsBold()} failed: {e.Message.AsBold()}");
                }
            }

            if (successUsers.Count > 0)
            {
                successBuilder.AppendLine(successUsers.Humanize());
            }

            if (successBuilder.ToString() != successBase)
            {
                finalBuilder.AppendLine(successBuilder.ToString());
            }

            if (errorBuilder.ToString() != ERROR_BASE)
            {
                finalBuilder.Append("\n\n" + errorBuilder);
            }

            var embed = new KaguyaEmbedBuilder(Color.Green)
            {
                Description = finalBuilder.ToString()
            };

            await SendEmbedAsync(embed);
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
                 "Lightblue, red, orange, purple, magenta, gold, yellow, lightred, lightgreen, pink, etc.\n" +
                 "If your color could not be found, the default Discord role color will be used.")]
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

        [Command("-createlist")]
        [Alias("-cl")]
        [Summary("Creates a list of roles with the names provided.\n" +
                 "If a role name has spaces, wrap it in \"quotation marks\".")]
        [Remarks("<role> [role] [...]")]
        public async Task CreateRolesCommand(params string[] names)
        {
            var createdBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();
            
            foreach (var name in names)
            {
                try
                {
                    await Context.Guild.CreateRoleAsync(name, null, null, false, null);
                    createdBuilder.AppendLine($"- {name.AsBold()}");
                }
                catch (Exception)
                {
                    errorBuilder.AppendLine($"- {name.AsBold()}");
                }
            }
            
            if (!string.IsNullOrWhiteSpace(createdBuilder.ToString()))
            {
                var embed = new KaguyaEmbedBuilder(Color.Green)
                            .WithDescription($"{Context.User.Mention} Roles created:\n\n" + createdBuilder)
                            .Build();

                await SendEmbedAsync(embed);
            }

            if (!string.IsNullOrWhiteSpace(errorBuilder.ToString()))
            {
                var embed = new KaguyaEmbedBuilder(Color.Green)
                            .WithDescription($"{Context.User.Mention} Failed to create roles:\n\n" + createdBuilder)
                            .Build();
                
                await SendEmbedAsync(embed);
            }
        }

        [Command("-delete")]
        [Alias("-d")]
        [Summary("Deletes a role or list of roles from the server. If a role name has spaces, " +
                 "wrap it in \"quotation marks\".")]
        [Remarks("<role> [role] [...]")]
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
                finalBuilder.AppendLine($"{Context.User.Mention} Roles deleted:")
                            .AppendLine(successBuilder.ToString());
            }

            if (failure)
            {
                finalBuilder.AppendLine($"{Context.User.Mention} Failed to delete roles:")
                            .AppendLine(errorBuilder.ToString());
            }

            Color color = default;
            if (success && failure)
            {
                color = Color.DarkMagenta;
            }
            else if (success)
            {
                color = Color.Green;
            }
            else if (failure)
            {
                color = Color.Red;
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
                                                                           "This cannot be undone!".AsBoldUnderlined())
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
                await SendBasicEmbedAsync("No action will be taken.", Color.DarkBlue);
            }
        }

        // [Restriction(ModuleRestriction.PremiumOnly)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("-deleteunused")]
        [Summary("Deletes all unused roles from the server. These are roles that are not assigned to any users.")]
        public async Task DeleteUnusedRolesCommand()
        {
            var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);

            List<SocketRole> success = new List<SocketRole>();
            List<(SocketRole, string)> fail = new List<(SocketRole, string)>();

            var unusedRoles = Context.Guild.Roles
                                     .Where(x => !x.Members.Any() && x.Id != server.MuteRoleId && !x.IsManaged)
                                     .ToArray();

            if (!unusedRoles.Any())
            {
                await SendBasicErrorEmbedAsync("There are no unused roles.");

                return;
            }
            
            foreach (SocketRole role in unusedRoles)
            {
                try
                {
                    await role.DeleteAsync();
                    success.Add(role);
                }
                catch (Exception e)
                {
                    fail.Add((role, e.Message));
                }
            }

            var finalBuilder = new StringBuilder();

            if (success.Any())
            {
                var successBuilder = new StringBuilder("Deleted the following roles: \n\n");
                successBuilder.Append(success.Humanize(x => x.Name.AsBold()));

                finalBuilder.AppendLine(successBuilder + "\n");
            }

            if (fail.Any())
            {
                var failBuilder = new StringBuilder("Failed to delete the following roles: \n\n");
                foreach ((SocketRole role, string reason) in fail)
                {
                    string reasonCopy = reason;
                    reasonCopy = reasonCopy.Replace("The server responded with ", "");
                    
                    string roleHumanized = role.Name.AsBold();
                    string error = "\nReason: " + reasonCopy.Humanize(LetterCasing.Sentence).AsItalics();
                    
                    failBuilder.AppendLine("- " + roleHumanized + error);
                }
            
                finalBuilder.AppendLine(failBuilder.ToString());
            }

            Color color = default;

            bool successOnly = success.Any() && !fail.Any();
            bool failureOnly = !success.Any() && fail.Any();
            bool mixed = success.Any() && fail.Any();
            
            if (mixed)
            {
                color = Color.DarkMagenta;
            }
            else if (successOnly)
            {
                color = Color.Green;
            }
            else if (failureOnly)
            {
                color = Color.Red;
            }

            var embedBuilder = new KaguyaEmbedBuilder(color)
                .WithDescription(finalBuilder.ToString());

            if (failureOnly)
            {
                embedBuilder.Footer = new EmbedFooterBuilder
                {
                    Text = "Errors often come from lack of permissions. Ensure the \"Kaguya\" role is at the top of the role hierarchy."
                };
            }

            await SendEmbedAsync(embedBuilder);
        }

        [Command("-rename")]
        [Summary("Renames the provided role to the name specified. If the **first** role name has spaces, wrap it " +
                 "with quotation marks. If the **new name** has spaces, do not wrap it with quotation marks.\n\n")]
        [Remarks("<role> <new name>")]
        [Example("penguins birds that can't fly")]
        [Example("\"birds that can't fly\" ice chicken")]
        public async Task RenameRoleCommand(SocketRole role, [Remainder]string newName)
        {
            try
            {
                await role.ModifyAsync(x => x.Name = newName);
            }
            catch (Exception e)
            {
                await SendBasicErrorEmbedAsync($"Failed to rename role {role.Mention}.\n" +
                                               $"Error: {e.Message.AsBold()}");

                return;
            }

            await SendBasicSuccessEmbedAsync($"Renamed the role successfully. New: {role.Mention}");
        }
    }
}