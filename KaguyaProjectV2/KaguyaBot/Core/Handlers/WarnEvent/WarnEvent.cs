using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using System;

// ReSharper disable VariableHidesOuterVariable
namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.WarnEvent
{
    public static class WarnEvent
    {
        public static event EventHandler<WarnHandlerEventArgs> OnWarn;
        public static void Trigger(Server server, WarnedUser warnedUser) => WarnEventTrigger(new WarnHandlerEventArgs(server, warnedUser));
        private static void WarnEventTrigger(WarnHandlerEventArgs e) => OnWarn?.Invoke(null, e);
    }

    public class WarnHandlerEventArgs : EventArgs
    {
        public WarnHandlerEventArgs(Server server, WarnedUser warnedUser)
        {
            Server = server;
            WarnedUser = warnedUser;
        }

        public Server Server { get; }
        public WarnedUser WarnedUser { get; }
    }
}