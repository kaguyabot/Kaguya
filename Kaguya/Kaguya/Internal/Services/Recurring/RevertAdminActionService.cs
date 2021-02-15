using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Humanizer;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kaguya.Internal.Services.Recurring
{
    /// <summary>
    /// Reverts various timed admin actions users may configure.
    ///
    /// i.e $mute (user) 30 minutes..after 30 minutes, the user must be unmuted.
    /// This sort of task is what this service is responsible for.
    /// </summary>
    public class RevertAdminActionService : BackgroundService, ITimerReceiver
    {
        private readonly ILogger<RevertAdminActionService> _logger;
        private readonly ITimerService _timerService;
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordShardedClient _client;
        private readonly SilentSysActions _sysActions;

        public RevertAdminActionService(ILogger<RevertAdminActionService> logger, ITimerService timerService, 
            IServiceProvider serviceProvider, DiscordShardedClient client, SilentSysActions sysActions)
        {
            _logger = logger;
            _timerService = timerService;
            _serviceProvider = serviceProvider;
            _client = client;
            _sysActions = sysActions;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            await _timerService.TriggerAtAsync(DateTime.Now, this);
        }

        public async Task HandleTimer(object payload)
        {
            await _timerService.TriggerAtAsync(DateTime.Now.AddSeconds(5), this);

            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                var adminActionRepository = scope.ServiceProvider.GetRequiredService<AdminActionRepository>();
                var kaguyaServerRepository = scope.ServiceProvider.GetRequiredService<KaguyaServerRepository>();
                
                var toUndo = await adminActionRepository.GetAllToUndoAsync();

                foreach (AdminAction action in toUndo)
                {
                    var socketGuild = _client.GetGuild(action.ServerId);

                    if (socketGuild == null)
                    {
                        _logger.LogDebug($"Failed to un-do admin action, guild {action.ServerId} was null");
                        continue;
                    }

                    var socketGuildUser = socketGuild.GetUser(action.ActionedUserId);
                    
                    if (socketGuildUser == null && !action.Action.Equals(AdminAction.BanAction, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogDebug($"Failed to un-do admin action, " +
                                         $"socketguilduser {action.ActionedUserId} was null");
                        continue;
                    }

                    var server = await kaguyaServerRepository.GetOrCreateAsync(socketGuild.Id);
                    
                    ulong? roleId;
                    ModerationAction modAction;
                    
                    switch (action.Action)
                    {
                        case AdminAction.MuteAction:
                            roleId = server.MuteRoleId;
                            modAction = ModerationAction.Mute;

                            break;
                        case AdminAction.ShadowbanAction:
                            roleId = server.ShadowbanRoleId;
                            modAction = ModerationAction.Shadowban;

                            break;
                        case AdminAction.BanAction:
                            roleId = null;
                            modAction = ModerationAction.Ban;

                            break;
                        default:
                            continue;
                    }

                    if (modAction == ModerationAction.Ban)
                    {
                        try
                        {
                            await socketGuild.RemoveBanAsync(action.ActionedUserId);
                            _logger.LogInformation($"Silently unbanned user {action.ActionedUserId}");
                        }
                        catch (Exception e)
                        {
                            _logger.LogWarning(e, $"Failed to silently unban user {action.ActionedUserId} in " +
                                                  $"guild {socketGuild.Id}.");
                        }
                    }
                    else
                    {
                        if (!roleId.HasValue)
                        {
                            _logger.LogWarning($"Failed to silently " +
                                               $"un{modAction.Humanize(LetterCasing.LowerCase)} " +
                                               $"user {socketGuildUser.Id} in guild {socketGuild.Id}. Role id " +
                                               $"did not have a value");
                        }
                        else
                        {
                            await _sysActions.SilentRemoveRoleByIdAsync(socketGuildUser, socketGuild, roleId.Value);
                            _logger.LogInformation($"Silently removed {modAction.Humanize(LetterCasing.LowerCase)} " +
                                                   $"from user {socketGuildUser.Id} in guild {socketGuild.Id}");
                        }
                    }
                    
                    action.HasTriggered = true;
                    await adminActionRepository.UpdateAsync(action);
                }
            }
        }
    }
}