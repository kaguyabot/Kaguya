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
            if(!((context.User as SocketGuildUser).GuildPermissions.Administrator || context.User.Id == 146092837723832320))
                return PreconditionResult.FromError("You must be a server Administrator to use this command.");
            return PreconditionResult.FromSuccess();
        }
    }
}