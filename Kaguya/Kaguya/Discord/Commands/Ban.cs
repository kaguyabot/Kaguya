using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands
{
    [Group("ban")]
    [Alias("b")]
    public class Ban : KaguyaBase
    {
        private readonly ILogger<Ban> _logger;

        public Ban(ILogger<Ban> logger) : base(logger)
        {
            _logger = logger;
        }
        
        [Command("")]
        [Summary("Permanently a user from the server.")]
        [Remarks("<user> [reason]")]
        public async Task CommandBan(KaguyaServerRepository repo, SocketGuildUser user, 
            [Remainder]string reason = "<No reason provided.>")
        {
	        KaguyaServer server = null;
            try
            {
                server = await repo.GetOrCreateAsync(Context.Guild.Id);
                server.TotalAdminActions++;

                await repo.UpdateAsync(server);
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
        public async Task CommandUnban(KaguyaServerRepository repo, IUser user, 
                                       [Remainder]string reason = "<No reason provided.>")
        {
	        try
	        {
		        var server = await repo.GetOrCreateAsync(Context.Guild.Id);
		        server.TotalAdminActions++;

		        await repo.UpdateAsync(server);
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
        [Summary("Temporarily bans the user from the server, for the time specified.")]
        [Remarks("<user> <duration> [reason]")]
        
        public async Task CommandTempban(KaguyaServerRepository ksRepo, TemporaryBanRepository tbRepo, IUser user, string timeString,
                                         [Remainder]string reason = "<No reason provided.>")
        {
	        // TODO: Parse time string.
	        try
	        {
		        // TODO: Create temporary ban object, insert into tbRepo.
		        // TODO: Increase server admin actions by one.
		        // TODO: Ensure service watches for new temporary bans and unbans when the time expires.
	        }
	        catch (Exception e)
	        {
		        Console.WriteLine(e);

		        throw;
	        }
        }
    }
}