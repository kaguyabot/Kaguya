using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Humanizer.Localisation;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Attributes;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Discord.Parsers;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands.Administration
{
    [Module(CommandModule.Administration)]
    [Group("mute")]
    [Alias("m")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireUserPermission(GuildPermission.KickMembers)]
    [RequireUserPermission(GuildPermission.MuteMembers)]
    [RequireUserPermission(GuildPermission.DeafenMembers)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.KickMembers)]
    [RequireBotPermission(GuildPermission.MuteMembers)]
    [RequireBotPermission(GuildPermission.DeafenMembers)]
    public class Mute : KaguyaBase<Mute>
    {
        private readonly ILogger<Mute> _logger;
        private readonly AdminActionRepository _adminActionRepository;
        private readonly KaguyaServerRepository _kaguyaServerRepository;

        public Mute(ILogger<Mute> logger, AdminActionRepository adminActionRepository, KaguyaServerRepository kaguyaServerRepository) : base(logger)
        {
            _logger = logger;
            _adminActionRepository = adminActionRepository;
            _kaguyaServerRepository = kaguyaServerRepository;
        }

        // If the user executes $mute <user> foo -- a 1-word reason.
        [Command]
        public async Task MuteCommand(SocketGuildUser user, string reason) => await MuteCommand(user, null, reason);
        
        [Priority(2)]
        [Command]
        [Summary("Mutes a user for an optional duration with an optional reason.")]
        [Remarks("<user> [duration] [reason]")]
        public async Task MuteCommand(SocketGuildUser user, string duration = null, [Remainder]string reason = "<No reason provided>")
        {
            KaguyaServer server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
            DateTime? muteExpiration = null;
            if (duration != null)
            {
                var timeParser = new TimeParser(duration);
                var parsedDuration = timeParser.ParseTime();

                if (parsedDuration > TimeSpan.Zero)
                {
                    muteExpiration = DateTime.Now.Add(parsedDuration);
                }

                if (muteExpiration == null && duration != null)
                {
                    reason = duration;
                }
            }

            // TODO: Test once ratelimit is over.
            await MuteUser(user, muteExpiration, reason, server);
        }

        private async Task MuteUser(SocketGuildUser user, DateTime? expiration, string reason, KaguyaServer server)
        {
            var adminAction = new AdminAction
            {
                ServerId = Context.Guild.Id,
                ModeratorId = Context.User.Id,
                ActionedUserId = user.Id,
                Action = AdminAction.MuteAction,
                Reason = reason,
                Expiration = expiration
            };

            await _adminActionRepository.InsertAsync(adminAction);

            bool muteRoleExists = DetermineIfMuteRoleExists(server);
            bool updateServer = false;
            
            IRole muteRole = await GetMuteRoleAsync(server);
            await user.AddRoleAsync(muteRole);

            if (!muteRoleExists)
            {
                updateServer = true;
                server.MuteRoleId = muteRole.Id;
            }

            if (updateServer)
            {
                await _kaguyaServerRepository.UpdateAsync(server);
            }

            Embed embed = GetFinalEmbed(user, expiration);
            await SendEmbedAsync(embed);
        }

        private bool DetermineIfMuteRoleExists(KaguyaServer server)
        {
            SocketRole muteRole = Context.Guild.GetRole(server.MuteRoleId);
            return muteRole != null;
        }

        private async Task<IRole> GetMuteRoleAsync(KaguyaServer server)
        {
            var match = Context.Guild.GetRole(server.MuteRoleId);

            if (match == null)
            {
                return await CreateMuteRoleAsync();
            }

            return match;
        }
        
        private async Task<IRole> CreateMuteRoleAsync()
        {
            return await Context.Guild.CreateRoleAsync("kaguya-mute", GuildPermissions.None, Color.Default, false, false, null);
        }

        private Embed GetFinalEmbed(SocketGuildUser target, DateTime? expiration)
        {
            string durationStr = expiration.HasValue 
                ? $" for {(expiration.Value - DateTime.Now).Humanize(3, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day).AsBold()}" 
                : string.Empty;

            return new KaguyaEmbedBuilder(Color.Purple)
                   .WithDescription($"{Context.User.Mention} successfully muted user {target.Mention}{durationStr}.")
                   .WithFooter("To unmute this user, use the mute -u command.")
                   .Build();
        }
    }
}