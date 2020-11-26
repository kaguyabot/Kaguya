using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Attributes;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Discord.Parsers;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands
{
	[Module(CommandModule.Administration)]
    [Group("ban")]
    [Alias("b")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    public class Ban : KaguyaBase<Ban>
    {
        private readonly ILogger<Ban> _logger;
        private readonly KaguyaServerRepository _ksRepo;
        private readonly AdminActionRepository _aaRepo;

        public Ban(ILogger<Ban> logger, KaguyaServerRepository ksRepo,
                   AdminActionRepository aaRepo) : base(logger)
        {
	        _logger = logger;
	        _ksRepo = ksRepo;
	        _aaRepo = aaRepo;
        }
        
        [Command]
        [Summary("Permanently bans a user from the server.")]
        [Remarks("<user> [reason]")]
        public async Task CommandBan(SocketGuildUser user, [Remainder]string reason = "<No reason provided.>")
        {
	        KaguyaServer server = null;
            try
            {
	            await BanAsync(user, reason);

	            await SendAsync($"{Context.User.Mention} Banned **{user}**.");
                
                // TODO: Trigger ban event.
            }
            catch (Exception e)
            {
	            var errorString = new StringBuilder()
	                                 .AppendLine($"{Context.User.Mention} Failed to ban user {user.ToString().AsBold()}.")
	                                 .AppendLine("Do I have enough permissions?".AsItalics())
	                                 .AppendLine("This error can also occur if the user you are trying to ban has more permissions than me.".AsItalics())
	                                 .AppendLine("Ensure my role is also at the top of the role heirarchy, then try again.".AsItalics())
	                                 .Append($"Error: {e.Message.AsBold()}")
	                                 .ToString();
	            
	            var embed = new KaguyaEmbedBuilder(Color.Red)
                {
                    Description = errorString
                };
	            
                await SendEmbedAsync(embed);
                
                _logger.LogDebug(e, "Exception encountered with ban in guild " + server.ServerId);
            }
        }

        private async Task BanAsync(SocketGuildUser user, string reason)
        {
	        var server = await _ksRepo.GetOrCreateAsync(Context.Guild.Id);
	        server.TotalAdminActions++;

	        await _ksRepo.UpdateAsync(server);

	        await user.BanAsync(reason: reason);
        }

        [Command("-u")]
        [Summary("Unbans the user from the server.")]
        [Remarks("<user id> [reason]")]
        public async Task CommandUnban(ulong id, [Remainder]string reason = "<No reason provided.>")
        {
	        try
	        {
		        var server = await _ksRepo.GetOrCreateAsync(Context.Guild.Id);
		        server.TotalAdminActions++;
		        
		        // Try to get the name of the user for display, if they exist.
		        var actionedUser = await Context.Guild.GetBanAsync(id);
		        
		        await _ksRepo.UpdateAsync(server);
		        await Context.Guild.RemoveBanAsync(id);

		        await SendAsync($"{Context.User.Mention} Unbanned user {actionedUser?.User.ToString().AsBold() ?? id.ToString().AsBold()}.");
			
		        // TODO: Trigger unban event.
	        }
	        catch (Exception e)
	        {
		        await SendAsync($"{Context.User.Mention} Failed to unban user with id {id.ToString().AsBold()}. Error: {e.Message.AsBold()}");
		        _logger.LogDebug(e, $"Exception encountered with ban in guild {Context.Guild}.");
	        }
        }

        [Command("-t")]
        [Summary("Temporarily bans the user from the server for the time specified.")]
        [Remarks("<user> <duration> [reason]")]
        public async Task CommandTempban(SocketGuildUser user, string timeString, [Remainder]string reason = "<No reason provided.>")
        {
	        var timeParser = new TimeParser(timeString);
	        var parsedTime = timeParser.ParseTime();
	        if (parsedTime == TimeSpan.Zero)
	        {
		        await SendAsync($"{Context.User.Mention} failed to temp-ban user **{user}**.\n" +
		                        $"`{timeString}` could not be parsed into a duration.");
		        return;
	        }
		        
	        try
	        {
		        // TODO: Create temporary ban object, insert into tbRepo.

		        await BanAsync(user, reason);

		        var adminAction = new AdminAction
					              {
					                  ServerId = Context.Guild.Id,
					                  ModeratorId = Context.User.Id,
					                  ActionedUserId = user.Id,
					                  Action = AdminAction.TEMP_BAN_ACTION,
					                  Expiration = DateTime.Now + parsedTime
					              };

		        await _aaRepo.InsertAsync(adminAction);

		        // TODO: Respond to user.
		        // TODO: Fix weird indentation in code style settings.
		        // TODO: Ensure service watches for new temporary bans and unbans when the time expires.
	        }
	        catch (Exception e)
	        {
		        _logger.LogDebug(e, $"Failed to ban user {user.Id} in guild {Context.Guild.Id}.");
		        await SendAsync($"{Context.User.Mention} Failed to ban user **{user}**. Reason: {e.Message}.");
	        }
        }
    }
}