using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace KaguyaProjectV2.KaguyaBot.Core.TypeReaders
{
    public class ListSocketGuildUserTr : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var result = new List<SocketGuildUser>();
            var allGuildUsers = await context.Guild
                .GetUsersAsync();

            var users = input
                .ToLower()
                .Split(' ');

            foreach (var baseUser in users)
            {
                SocketGuildUser user;
                ulong id;

                if (MentionUtils.TryParseUser(baseUser, out id))
                    user = await context.Channel.GetUserAsync(id) as SocketGuildUser;

                else if (ulong.TryParse(baseUser, out id))
                    user = await context.Channel.GetUserAsync(id) as SocketGuildUser;

                else
                    user = allGuildUsers
                            .FirstOrDefault(u =>
                                u.Username?.ToLower() == baseUser ||
                                u.Nickname?.ToLower() == baseUser)
                        as SocketGuildUser;

                if (user == null)
                    return TypeReaderResult.FromError(CommandError.ParseFailed, $"User ID: {baseUser} could not be found.");
                else
                    result.Add(user);
            }

            if (result.Count != 0)
                return TypeReaderResult.FromSuccess(result);
            else
                return TypeReaderResult.FromError(CommandError.ParseFailed, $"Input could not be parsed. [ListSocketGuildUserTR].");
        }
    }
}