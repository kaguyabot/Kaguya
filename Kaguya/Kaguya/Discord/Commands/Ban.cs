using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Parsers;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands
{
	// TODO: Categorize as AdministrationCommand with attribute [AdminCommand]
    [Group("ban")]
    [Alias("b")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    public class Ban : KaguyaBase<Ban>
    {
        private readonly ILogger<Ban> _logger;
        private readonly KaguyaServerRepository _ksRepo;

        public Ban(ILogger<Ban> logger, KaguyaServerRepository ksRepo) : base(logger)
        {
	        _logger = logger;
	        _ksRepo = ksRepo;
        }
        
        [Command]
        [Summary("Permanently bans a user from the server.")]
        [Remarks("<user> [reason]")]
        public async Task CommandBan(SocketGuildUser user, [Remainder]string reason = "<No reason provided.>")
        {
	        KaguyaServer server = null;
            try
            {
                server = await _ksRepo.GetOrCreateAsync(Context.Guild.Id);
                server.TotalAdminActions++;

                await _ksRepo.UpdateAsync(server);
                await user.BanAsync(reason: reason);
                
                await SendAsync($"{Context.User.Mention} Banned **{user}**.");
                
                // TODO: Trigger ban event.
            }
            catch (Exception e)
            {
                await SendAsync($"{Context.User.Mention} Failed to ban user {user}. Error: {e.Message}");
                _logger.LogDebug(e, "Exception encountered with ban in guild " +
                                    $"{(server == null ? "NULL" : server.ServerId)}.");
            }
        }

        [Command("-u")]
        [Summary("Unbans the user from the server.")]
        [Remarks("<user> [reason]")]
        public async Task CommandUnban(SocketGuildUser user, [Remainder]string reason = "<No reason provided.>")
        {
	        try
	        {
		        var server = await _ksRepo.GetOrCreateAsync(Context.Guild.Id);
		        server.TotalAdminActions++;

		        await _ksRepo.UpdateAsync(server);
		        await Context.Guild.RemoveBanAsync(user);

		        await SendAsync($"{Context.User.Mention} Unbanned user **{user}**.");

		        // TODO: Trigger unban event.
	        }
	        catch (Exception e)
	        {
		        await SendAsync($"{Context.User.Mention} Failed to unban user {user}. Error: {e.Message}");
		        _logger.LogDebug(e, "Exception encountered with ban in guild IMPLEMENT.");
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

		        var server = await _ksRepo.GetOrCreateAsync(Context.Guild.Id);
		        server.TotalAdminActions++;

		        await _ksRepo.UpdateAsync(server);
		        
		        // TODO: Ensure service watches for new temporary bans and unbans when the time expires.

		        await user.BanAsync(reason: reason);
	        }
	        catch (Exception e)
	        {
		        _logger.LogDebug(e, $"Failed to ban user {user.Id} in guild {Context.Guild.Id}.");
		        await SendAsync($"{Context.User.Mention} Failed to ban user **{user}**. Reason: {e.Message}.");
	        }
        }
    }
}