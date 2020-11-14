using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using System;
using System.Threading.Tasks;

// ReSharper disable VariableHidesOuterVariable
namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.WarnEvent
{
    public static class WarnEvent
    {
        public static event Func<WarnHandlerEventArgs, Task> OnWarn;
        public static void Trigger(Server server, WarnedUser warnedUser) => WarnEventTrigger(new WarnHandlerEventArgs(server, warnedUser));
        private static void WarnEventTrigger(WarnHandlerEventArgs e) => OnWarn?.Invoke(e);
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