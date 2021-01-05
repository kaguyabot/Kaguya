using System.Collections.Generic;
using System.Linq;

namespace Kaguya.Discord.Memory
{
    public enum MultiplayerGameType
    {
        CrossGambling
    }
    
    /// <summary>
    /// Responsible for the handling of all multiplayer game sessions.
    /// </summary>
    public static class ActiveMultiplayerSessions
    {
        private static readonly IList<IMultiplayerSession> _activeSessions = new List<IMultiplayerSession>();
        public static IMultiplayerSession GetSession(ulong channelId) => _activeSessions.FirstOrDefault(x => x.ChannelId == channelId);
        public static void AddSession(IMultiplayerSession session) => _activeSessions.Add(session);
        public static void RemoveSession(IMultiplayerSession session) => _activeSessions.Remove(session);
        public static bool IsActive(ulong channelId, MultiplayerGameType type) =>
            _activeSessions.Any(x => x.ChannelId == channelId && x.GameType == type);
    }

    public class CrossGamblingSession : IMultiplayerSession
    {
        public MultiplayerGameType GameType { get; init; }
        public ulong ChannelId { get; init; }

        public CrossGamblingSession(ulong channelId, MultiplayerGameType gameType)
        {
            this.ChannelId = channelId;
            this.GameType = gameType;
        }
    }

    public interface IMultiplayerSession
    {
        public MultiplayerGameType GameType { get; init; }
        /// <summary>
        /// Multiplayer sessions are unique per channel id.
        /// </summary>
        public ulong ChannelId { get; init; }
    }
}