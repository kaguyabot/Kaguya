using System;
using Kaguya.Internal.Events.ArgModels;
using OsuSharp;

namespace Kaguya.Internal.Events
{
    public static class KaguyaEventDefinitions
    {
        public class AntiraidEvent
        {
            public static event Action<AntiraidEventArgs> OnRaid;
            public static void Trigger(AntiraidEventArgs args) => OnRaid?.Invoke(args);
            
            // All other events and triggers will live here.
        }
    }
}