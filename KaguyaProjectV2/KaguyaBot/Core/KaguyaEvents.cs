using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Services;

namespace KaguyaProjectV2.KaguyaBot.Core
{
    public static class KaguyaEvents
    {
        /// <summary>
        /// Fired when a guild's antiraid service is triggered.
        /// </summary>
        public static event Func<AntiRaidEventArgs, Task> OnRaid;
        public static void TriggerAntiraid(AntiRaidEventArgs e) => OnRaid?.Invoke(e);
    }
}