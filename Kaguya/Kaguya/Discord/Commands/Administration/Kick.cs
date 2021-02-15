using System;
using System.Text;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;

namespace Kaguya.Discord.Commands.Administration
{
    [Module(CommandModule.Administration)]
    [Group("kick")]
    [Alias("k")]
    [RequireUserPermission(GuildPermission.KickMembers)]
    [RequireBotPermission(GuildPermission.KickMembers)]
    public class Kick : KaguyaBase<Kick>
    {
        private readonly ILogger<Kick> _logger;
        private readonly AdminActionRepository _adminActionRepository;
        private readonly KaguyaServerRepository _kaguyaServerRepository;

        public Kick(ILogger<Kick> logger, AdminActionRepository adminActionRepository, KaguyaServerRepository kaguyaServerRepository) : base(logger)
        {
	        _logger = logger;
	        _adminActionRepository = adminActionRepository;
	        _kaguyaServerRepository = kaguyaServerRepository;
        }

        [Command]
        [Summary("Kicks a user from the server.")]
        [Remarks("<user> [reason]")]
        public async Task KickCommand(SocketGuildUser user, [Remainder]string reason = null)
        {
	        KaguyaServer server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
            try
            {
	            var adminAction = new AdminAction
	            {
		            ServerId = Context.Guild.Id,
		            ModeratorId = Context.User.Id,
		            ActionedUserId = user.Id,
		            Action = AdminAction.KickAction,
		            Reason = reason,
		            Expiration = null
	            };
	            
	            // Kicks the user *and* updates the server admin actions in DB.
	            await KickAsync(user, adminAction, reason);
	            await SendBasicSuccessEmbedAsync($"Kicked **{user}**.");
	            // TODO: Trigger kick event.
            }
            catch (Exception e)
            {
	            string errorString = new StringBuilder()
	                                 .AppendLine($"{Context.User.Mention} Failed to kick user {user.ToString().AsBold()}.")
	                                 .AppendLine("Do I have enough permissions?".AsItalics())
	                                 .AppendLine("This error can also occur if the user you are trying to kick has more permissions than me.".AsItalics())
	                                 .AppendLine("Ensure my role is also at the top of the role heirarchy, then try again.".AsItalics())
	                                 .Append($"Error: {e.Message.AsBold()}")
	                                 .ToString();
	            
                await SendBasicErrorEmbedAsync(errorString);
                
                _logger.LogDebug(e, "Exception encountered with kick in guild " + server.ServerId);
            }
        }

        private async Task KickAsync(SocketGuildUser user, AdminAction action, string reason)
        {
	        var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
	        server.TotalAdminActions++;

	        await _adminActionRepository.InsertAsync(action);
	        await _kaguyaServerRepository.UpdateAsync(server);

	        await user.KickAsync(reason);
        }
    }
}