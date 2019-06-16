using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

#pragma warning disable

namespace Kaguya.Core.Attributes
{
    public class RequireAdminAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if((context.User as SocketGuildUser).GuildPermissions.Administrator)
                return PreconditionResult.FromSuccess();
            return PreconditionResult.FromError("You must be a server Administrator to use this command.");
        }
    }
}