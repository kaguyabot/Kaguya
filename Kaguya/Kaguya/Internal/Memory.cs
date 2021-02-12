using System.Collections.Concurrent;

namespace Kaguya.Internal
{
    /// <summary>
    /// A class used for cross-file caching and thread-safe collections.
    /// </summary>
    public static class Memory
    {
        public static readonly ConcurrentDictionary<ulong, bool> ServersCurrentlyPurgingMessages = new();
    }
}