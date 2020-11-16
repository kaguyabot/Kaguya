using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Interfaces
{
    public interface IModeratorEventArgs
    {
        Server Server { get; }
        SocketGuild Guild { get; }
        SocketGuildUser ActionedUser { get; }
        SocketGuildUser ModeratorUser { get; }
        string Reason { get; }
    }
}